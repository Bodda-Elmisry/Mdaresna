using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.Schools.Infrastructure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Mdaresna.Schools.Infrastructure.Messaging;

public sealed class SchoolRoutingCacheConsumerService(IConfiguration config, IHostEnvironment environment,
    IServiceScopeFactory scopes, ILogger<SchoolRoutingCacheConsumerService> logger) : BackgroundService
{
    internal const string Exchange = "mdaresna.platform-events";
    internal const string Queue = "schools.routing-cache.v1";
    private const string DeadQueue = "schools.routing-cache.v1.dead";
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        if (!config.GetValue<bool>("SchoolRoutingCache:Enabled") || !(config.GetValue<bool?>("SchoolRoutingCacheConsumer:Enabled") ?? true)) return;
        if (!Uri.TryCreate(config["SchoolRoutingCacheConsumer:BrokerUri"], UriKind.Absolute, out var uri) ||
            (uri.Scheme != "amqps" && !(environment.IsDevelopment() && uri.Scheme == "amqp")))
            throw new InvalidOperationException("Routing cache consumer requires AMQPS; AMQP is allowed only in Development.");
        var factory = new ConnectionFactory { Uri = uri, AutomaticRecoveryEnabled = true, TopologyRecoveryEnabled = true };
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await using var connection = await factory.CreateConnectionAsync(ct);
                await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true), ct);
                await channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, true, false, null, cancellationToken: ct);
                await channel.QueueDeclareAsync(Queue, true, false, false, null, cancellationToken: ct);
                await channel.QueueBindAsync(Queue, Exchange, SchoolRoutingChangedV1.MessageType, arguments: null, cancellationToken: ct);
                await channel.QueueDeclareAsync(DeadQueue, true, false, false, null, cancellationToken: ct);
                await channel.BasicQosAsync(0, 1, false, ct);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, message) => await Handle(channel, message, ct);
                await channel.BasicConsumeAsync(Queue, false, consumer, ct);
                while (connection.IsOpen && channel.IsOpen && !ct.IsCancellationRequested) await Task.Delay(TimeSpan.FromSeconds(2), ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { return; }
            catch (Exception) { logger.LogWarning("Routing cache consumer unavailable; retrying."); await Task.Delay(TimeSpan.FromSeconds(5), ct); }
        }
    }

    private async Task Handle(IChannel channel, BasicDeliverEventArgs delivery, CancellationToken ct)
    {
        var stage = "message-validation";
        try
        {
            SchoolRoutingChangedV1 changed;
            try
            {
                if (delivery.Body.Length > 65536 || delivery.RoutingKey != SchoolRoutingChangedV1.MessageType) throw new JsonException();
                var envelope = IntegrationJsonSerializer.Deserialize<SchoolRoutingChangedV1>(Encoding.UTF8.GetString(delivery.Body.Span));
                if (envelope.Producer != "mdaresna-platform" || envelope.SchemaVersion != 1) throw new JsonException();
                changed = envelope.Data;
                Validate(changed);
                if (envelope.Scope.SchoolId?.Value != changed.SchoolId || envelope.Scope.TenantId?.Value != changed.TenantId)
                    throw new ArgumentException("Routing scope does not match its payload.");
            }
            catch (Exception ex) when (ex is ArgumentException or JsonException)
            {
                await channel.BasicPublishAsync("", DeadQueue, true, new BasicProperties { Persistent = true }, delivery.Body, ct);
                await channel.BasicAckAsync(delivery.DeliveryTag, false, ct);
                return;
            }
            await using var scope = scopes.CreateAsyncScope();
            var client = scope.ServiceProvider.GetRequiredService<SchoolRoutingSnapshotClient>();
            stage = "platform-snapshot";
            // Reconcile only on mutation events. This prevents stale redeliveries after Redis loses its guards.
            var latest = await client.ReadAsync(changed.SchoolCode, ct);
            if (latest is null) latest = changed with { Target = null }; // A physically removed school stays blocked.
            Validate(latest);
            if (latest.SchoolId != changed.SchoolId)
            {
                // A stale event for a removed/replaced identity must not block the entire queue.
                await channel.BasicPublishAsync("", DeadQueue, true, new BasicProperties { Persistent = true }, delivery.Body, ct);
                await channel.BasicAckAsync(delivery.DeliveryTag, false, ct);
                return;
            }
            if (latest.SchoolCode != changed.SchoolCode || latest.RoutingVersion < changed.RoutingVersion)
                throw new InvalidOperationException("Routing snapshot is not current.");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct); timeout.CancelAfter(TimeSpan.FromSeconds(5));
            stage = "redis-apply";
            await scope.ServiceProvider.GetRequiredService<SchoolRoutingCache>().ApplyEventAsync(latest, timeout.Token);
            stage = "message-ack";
            await channel.BasicAckAsync(delivery.DeliveryTag, false, ct);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex)
        {
            // Do not log exception messages: Redis/HTTP diagnostics may contain connection metadata.
            logger.LogWarning("Routing update failed at {Stage} ({FailureType}); retaining the message for retry.", stage, ex.GetType().Name);
            await Task.Delay(TimeSpan.FromSeconds(2), ct);
            await channel.BasicNackAsync(delivery.DeliveryTag, false, true, ct);
        }
    }
    internal static void Validate(SchoolRoutingChangedV1 snapshot)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (snapshot.SchoolId == Guid.Empty || snapshot.TenantId != snapshot.SchoolId || snapshot.RoutingVersion < 0 ||
            !Regex.IsMatch(snapshot.SchoolCode ?? "", "^[A-Z0-9](?:[A-Z0-9-]{1,30}[A-Z0-9])$")) throw new ArgumentException("Invalid routing snapshot.");
        if (snapshot.Target is { } t && (t.SchoolId != snapshot.SchoolId || t.TenantId != snapshot.TenantId ||
            t.SchoolCode != snapshot.SchoolCode || t.RoutingVersion != snapshot.RoutingVersion || t.Status != "Active" ||
            t.Provider is not ("PostgreSql" or "SqlServer") || t.Port is < 1 or > 65535 ||
            string.IsNullOrWhiteSpace(t.Host) || string.IsNullOrWhiteSpace(t.DatabaseName) || string.IsNullOrWhiteSpace(t.CredentialSecretReference)))
            throw new ArgumentException("Invalid routing target.");
    }
}
