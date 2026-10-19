using System.Text;
using System.Text.Json;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.Schools.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Infrastructure.Messaging;

public sealed class SchoolSubscriptionConsumerService(IConfiguration config, IHostEnvironment environment, IServiceScopeFactory scopes,
    ILogger<SchoolSubscriptionConsumerService> logger) : BackgroundService
{
    private const string Queue = "schools.subscriptions.v1";
    private const string Exchange = "mdaresna.platform-events";
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        if (!config.GetValue<bool>("SchoolSubscriptions:ConsumerEnabled")) return;
        var uri = config["SchoolSubscriptions:BrokerUri"] ?? config["SchoolProvisioningConsumer:BrokerUri"];
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var broker) || (broker.Scheme != "amqps" && !(environment.IsDevelopment() && broker.Scheme == "amqp")))
            throw new InvalidOperationException("School subscription events require AMQPS; Development permits AMQP.");
        var factory = new ConnectionFactory { Uri = broker, AutomaticRecoveryEnabled = true, TopologyRecoveryEnabled = true };
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await using var connection = await factory.CreateConnectionAsync(ct);
                await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true), ct);
                await channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, true, false, null, cancellationToken: ct);
                await channel.QueueDeclareAsync(Queue, true, false, false, null, cancellationToken: ct);
                await channel.QueueBindAsync(Queue, Exchange, SchoolSubscriptionSnapshotChangedV1.MessageType, arguments: null, cancellationToken: ct);
                await channel.QueueDeclareAsync(Queue + ".dead", true, false, false, null, cancellationToken: ct);
                await channel.ExchangeDeclareAsync("mdaresna.school-events", ExchangeType.Topic, true, false, null, cancellationToken: ct);
                await channel.QueueDeclareAsync("platform.subscriptions.applied.v1", true, false, false, null, cancellationToken: ct);
                await channel.QueueBindAsync("platform.subscriptions.applied.v1", "mdaresna.school-events", SchoolSubscriptionAppliedV1.MessageType, arguments: null, cancellationToken: ct);
                await channel.BasicQosAsync(0, 1, false, ct);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, delivery) => {
                    try
                    {
                        SchoolSubscriptionSnapshotChangedV1 snapshot;
                        try
                        {
                            if (delivery.Body.Length > 65536 || delivery.RoutingKey != SchoolSubscriptionSnapshotChangedV1.MessageType) throw new JsonException();
                            var envelope = IntegrationJsonSerializer.Deserialize<SchoolSubscriptionSnapshotChangedV1>(Encoding.UTF8.GetString(delivery.Body.Span));
                            snapshot = envelope.Data; SchoolSubscriptionSync.Validate(snapshot);
                            if (envelope.Producer != "mdaresna-platform" || envelope.SchemaVersion != 1 || envelope.Scope.SchoolId?.Value != snapshot.SchoolId || envelope.Scope.TenantId?.Value != snapshot.SchoolId) throw new JsonException();
                        }
                        catch (Exception ex) when (ex is JsonException or ArgumentException)
                        {
                            await channel.BasicPublishAsync("", Queue + ".dead", true, new BasicProperties { Persistent = true }, delivery.Body, ct);
                            await channel.BasicAckAsync(delivery.DeliveryTag, false, ct); return;
                        }
                        await using var scope = scopes.CreateAsyncScope();
                        await using var db = await scope.ServiceProvider.GetRequiredService<ISchoolDbContextFactory>().CreateAsync(snapshot.SchoolCode, ct)
                            ?? throw new InvalidOperationException("School database is not ready.");
                        try { await SchoolSubscriptionSync.ApplyAsync(db, snapshot, ct); }
                        catch (ArgumentException)
                        {
                            await channel.BasicPublishAsync("", Queue + ".dead", true, new BasicProperties { Persistent = true }, delivery.Body, ct);
                            await channel.BasicAckAsync(delivery.DeliveryTag, false, ct); return;
                        }
                        var applied = await db.Subscriptions.AsNoTracking().SingleAsync(x => x.SystemId == snapshot.SystemId, ct);
                        var receipt = IntegrationMessageEnvelope<SchoolSubscriptionAppliedV1>.Create(DateTimeOffset.UtcNow, "mdaresna-schools",
                            IntegrationMessageScope.ForSchool(TenantId.From(snapshot.SchoolId), SchoolId.From(snapshot.SchoolId)),
                            new SchoolSubscriptionAppliedV1(snapshot.SchoolId, snapshot.SystemId, applied.SubscriptionId, applied.Revision, applied.SyncedAtUtc));
                        // The original durable delivery remains unacknowledged until SQL commits AND the
                        // receipt publish is confirmed. A crash between them safely replays this path.
                        await channel.BasicPublishAsync("mdaresna.school-events", SchoolSubscriptionAppliedV1.MessageType, true,
                            new BasicProperties { Persistent = true, ContentType = "application/json", MessageId = receipt.MessageId.ToString("D") },
                            Encoding.UTF8.GetBytes(IntegrationJsonSerializer.Serialize(receipt)), ct);
                        await channel.BasicAckAsync(delivery.DeliveryTag, false, ct);
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
                    catch (Exception) { logger.LogWarning("Subscription snapshot was not applied; retrying without acknowledgement."); await Task.Delay(TimeSpan.FromSeconds(2), ct); await channel.BasicNackAsync(delivery.DeliveryTag, false, true, ct); }
                };
                await channel.BasicConsumeAsync(Queue, false, consumer, ct);
                while (connection.IsOpen && channel.IsOpen && !ct.IsCancellationRequested) await Task.Delay(TimeSpan.FromSeconds(2), ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { return; }
            catch (Exception) { logger.LogWarning("Subscription consumer is unavailable; retrying."); await Task.Delay(TimeSpan.FromSeconds(5), ct); }
        }
    }
}
