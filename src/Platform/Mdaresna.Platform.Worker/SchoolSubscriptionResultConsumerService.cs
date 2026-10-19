using System.Text;
using System.Text.Json;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.Platform.Application.Billing.Subscriptions;
using Mdaresna.Schools.Contracts.Provisioning;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Mdaresna.Platform.Worker;

internal sealed class SchoolSubscriptionResultConsumerService(IConfiguration config, IHostEnvironment environment,
    IServiceScopeFactory scopes, ILogger<SchoolSubscriptionResultConsumerService> logger) : BackgroundService
{
    private const string Exchange = "mdaresna.school-events", Queue = "platform.subscriptions.applied.v1";
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        if (!config.GetValue<bool>("SchoolSubscriptionResults:Enabled")) return;
        var uri = config["SchoolSubscriptionResults:BrokerUri"] ?? config["PlatformOutboxPublisher:BrokerUri"];
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var broker) || (broker.Scheme != "amqps" && !(environment.IsDevelopment() && broker.Scheme == "amqp")))
            throw new InvalidOperationException("Subscription receipts require AMQPS; Development permits AMQP.");
        var factory = new ConnectionFactory { Uri = broker, AutomaticRecoveryEnabled = true, TopologyRecoveryEnabled = true };
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await using var connection = await factory.CreateConnectionAsync(ct);
                await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true), ct);
                await channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, true, false, null, cancellationToken: ct);
                await channel.QueueDeclareAsync(Queue, true, false, false, null, cancellationToken: ct);
                await channel.QueueBindAsync(Queue, Exchange, SchoolSubscriptionAppliedV1.MessageType, arguments: null, cancellationToken: ct);
                await channel.QueueDeclareAsync(Queue + ".dead", true, false, false, null, cancellationToken: ct);
                await channel.BasicQosAsync(0, 1, false, ct);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, delivery) => {
                    try
                    {
                        if (delivery.Body.Length > 16384 || delivery.RoutingKey != SchoolSubscriptionAppliedV1.MessageType) throw new JsonException();
                        var envelope = IntegrationJsonSerializer.Deserialize<SchoolSubscriptionAppliedV1>(Encoding.UTF8.GetString(delivery.Body.Span));
                        var data = envelope.Data;
                        if (envelope.Producer != "mdaresna-schools" || envelope.SchemaVersion != 1 || data.SchoolId == Guid.Empty ||
                            data.SystemId == Guid.Empty || data.SubscriptionId == Guid.Empty || envelope.Scope.SchoolId?.Value != data.SchoolId ||
                            envelope.Scope.TenantId?.Value != data.SchoolId) throw new JsonException();
                        await using var scope = scopes.CreateAsyncScope();
                        await scope.ServiceProvider.GetRequiredService<ISubscriptionService>().RecordAppliedAsync(data.SchoolId, data.SystemId, data.SubscriptionId, data.Revision, data.AppliedAtUtc, ct);
                        await channel.BasicAckAsync(delivery.DeliveryTag, false, ct);
                    }
                    catch (Exception error) when (error is JsonException or ArgumentException)
                    {
                        await channel.BasicPublishAsync("", Queue + ".dead", true, new BasicProperties { Persistent = true }, delivery.Body, ct);
                        await channel.BasicAckAsync(delivery.DeliveryTag, false, ct);
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
                    catch (Exception)
                    {
                        logger.LogWarning("Subscription receipt was not stored; retrying without acknowledgement.");
                        await Task.Delay(TimeSpan.FromSeconds(2), ct); await channel.BasicNackAsync(delivery.DeliveryTag, false, true, ct);
                    }
                };
                await channel.BasicConsumeAsync(Queue, false, consumer, ct);
                while (connection.IsOpen && channel.IsOpen && !ct.IsCancellationRequested) await Task.Delay(TimeSpan.FromSeconds(2), ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { return; }
            catch (Exception) { logger.LogWarning("Subscription receipt consumer unavailable; retrying."); await Task.Delay(TimeSpan.FromSeconds(5), ct); }
        }
    }
}
