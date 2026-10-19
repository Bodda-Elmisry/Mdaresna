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

public sealed class SchoolInvoiceConsumerService(IConfiguration config, IHostEnvironment environment, IServiceScopeFactory scopes,
    ILogger<SchoolInvoiceConsumerService> logger) : BackgroundService
{
    private const string Queue = "schools.invoices.v1";
    private const string Exchange = "mdaresna.platform-events";
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        if (!config.GetValue<bool>("SchoolSubscriptions:ConsumerEnabled")) return;
        var uri = config["SchoolSubscriptions:BrokerUri"] ?? config["SchoolProvisioningConsumer:BrokerUri"];
        if (!Uri.TryCreate(uri, UriKind.Absolute, out var broker) || (broker.Scheme != "amqps" && !(environment.IsDevelopment() && broker.Scheme == "amqp")))
            throw new InvalidOperationException("School invoice events require AMQPS; Development permits AMQP.");
        var factory = new ConnectionFactory { Uri = broker, AutomaticRecoveryEnabled = true, TopologyRecoveryEnabled = true };
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await using var connection = await factory.CreateConnectionAsync(ct);
                await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true), ct);
                await channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, true, false, null, cancellationToken: ct);
                await channel.QueueDeclareAsync(Queue, true, false, false, null, cancellationToken: ct);
                await channel.QueueBindAsync(Queue, Exchange, SchoolInvoiceSnapshotChangedV1.MessageType, arguments: null, cancellationToken: ct);
                await channel.QueueDeclareAsync(Queue + ".dead", true, false, false, null, cancellationToken: ct);
                await channel.BasicQosAsync(0, 1, false, ct);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, delivery) => {
                    try
                    {
                        SchoolInvoiceSnapshotChangedV1 snapshot;
                        try
                        {
                            if (delivery.Body.Length > 1048576 || delivery.RoutingKey != SchoolInvoiceSnapshotChangedV1.MessageType) throw new JsonException();
                            var envelope = IntegrationJsonSerializer.Deserialize<SchoolInvoiceSnapshotChangedV1>(Encoding.UTF8.GetString(delivery.Body.Span));
                            snapshot = envelope.Data; SchoolInvoiceSync.Validate(snapshot);
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
                        try { await SchoolInvoiceSync.ApplyAsync(db, snapshot, ct); }
                        catch (ArgumentException)
                        {
                            await channel.BasicPublishAsync("", Queue + ".dead", true, new BasicProperties { Persistent = true }, delivery.Body, ct);
                            await channel.BasicAckAsync(delivery.DeliveryTag, false, ct); return;
                        }
                        await channel.BasicAckAsync(delivery.DeliveryTag, false, ct);
                    }
                    catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
                    catch (Exception) { logger.LogWarning("Invoice snapshot was not applied; retrying without acknowledgement."); await Task.Delay(TimeSpan.FromSeconds(2), ct); await channel.BasicNackAsync(delivery.DeliveryTag, false, true, ct); }
                };
                await channel.BasicConsumeAsync(Queue, false, consumer, ct);
                while (connection.IsOpen && channel.IsOpen && !ct.IsCancellationRequested) await Task.Delay(TimeSpan.FromSeconds(2), ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { return; }
            catch (Exception) { logger.LogWarning("Invoice consumer is unavailable; retrying."); await Task.Delay(TimeSpan.FromSeconds(5), ct); }
        }
    }
}

