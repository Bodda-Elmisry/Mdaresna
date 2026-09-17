using System.Text;
using System.Text.Json;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Application.Registry.DatabaseMigrations;
using Mdaresna.Platform.Domain.Common;
using Mdaresna.Schools.Contracts.Provisioning;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Mdaresna.Platform.Worker;

internal sealed class SchoolDatabaseMigrationResultConsumerService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<SchoolDatabaseMigrationResultConsumerService> logger) : BackgroundService
{
    private const string Exchange = "mdaresna.school-events";
    private const string Queue = "platform.school-database-migrated.v1";
    private const string DeadExchange = "mdaresna.school-events.dead";
    private const string DeadQueue = "platform.school-database-migrated.v1.dead";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuration.GetValue<bool>("SchoolProvisioningResultConsumer:Enabled"))
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            return;
        }
        var configuredUri = configuration["SchoolProvisioningResultConsumer:BrokerUri"];
        if (!Uri.TryCreate(configuredUri, UriKind.Absolute, out var brokerUri) ||
            (brokerUri.Scheme != "amqps" && !(environment.IsDevelopment() && brokerUri.Scheme == "amqp")))
            throw new InvalidOperationException("SchoolProvisioningResultConsumer:BrokerUri must use AMQPS (AMQP is allowed only in Development).");
        var factory = new ConnectionFactory { Uri = brokerUri, AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true, ClientProvidedName = "mdaresna-platform:database-migration-result-consumer" };
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var connection = await factory.CreateConnectionAsync(stoppingToken);
                await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
                await DeclareTopologyAsync(channel, stoppingToken);
                await channel.BasicQosAsync(0, 1, false, stoppingToken);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, delivery) => await HandleAsync(channel, delivery, stoppingToken);
                await channel.BasicConsumeAsync(Queue, false, consumer, stoppingToken);
                logger.LogInformation("Platform school database migration result consumer is subscribed.");
                while (!stoppingToken.IsCancellationRequested && connection.IsOpen && channel.IsOpen)
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "School database migration result consumer is unavailable; reconnecting.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private static async Task DeclareTopologyAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, true, false, null, cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(DeadExchange, ExchangeType.Direct, true, false, null, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(DeadQueue, true, false, false,
            new Dictionary<string, object?> { ["x-message-ttl"] = 86_400_000 }, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(DeadQueue, DeadExchange, DeadQueue, arguments: null, cancellationToken: cancellationToken);
        var arguments = new Dictionary<string, object?> { ["x-dead-letter-exchange"] = DeadExchange,
            ["x-dead-letter-routing-key"] = DeadQueue };
        await channel.QueueDeclareAsync(Queue, true, false, false, arguments, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(Queue, Exchange, SchoolDatabaseMigratedV1.MessageType,
            arguments: null, cancellationToken: cancellationToken);
    }

    private async Task HandleAsync(IChannel channel, BasicDeliverEventArgs delivery,
        CancellationToken cancellationToken)
    {
        try
        {
            if (delivery.Body.Length > 64 * 1024) throw new JsonException("Migration result is too large.");
            var envelope = IntegrationJsonSerializer.Deserialize<SchoolDatabaseMigratedV1>(
                Encoding.UTF8.GetString(delivery.Body.Span));
            if (delivery.RoutingKey != SchoolDatabaseMigratedV1.MessageType || envelope.Producer != "mdaresna-schools")
                throw new JsonException("Unexpected migration result source.");
            await using var scope = scopeFactory.CreateAsyncScope();
            await scope.ServiceProvider.GetRequiredService<CompleteSchoolDatabaseMigrationHandler>()
                .HandleAsync(envelope.Data, cancellationToken);
            await channel.BasicAckAsync(delivery.DeliveryTag, false, cancellationToken);
        }
        catch (Exception ex) when (ex is JsonException or ArgumentException or
                                   PlatformResourceNotFoundException or PlatformConflictException or
                                   PlatformDomainException)
        {
            logger.LogWarning(ex, "Invalid school database migration result moved to the dead-letter queue.");
            await channel.BasicRejectAsync(delivery.DeliveryTag, false, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "School database migration result persistence failed; delivery will be retried.");
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            await channel.BasicNackAsync(delivery.DeliveryTag, false, true, cancellationToken);
        }
    }
}
