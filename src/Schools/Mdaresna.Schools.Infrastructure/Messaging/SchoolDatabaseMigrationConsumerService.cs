using System.Text;
using System.Text.Json;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Mdaresna.Schools.Infrastructure.Messaging;

public sealed class SchoolDatabaseMigrationConsumerService(
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<SchoolDatabaseMigrationConsumerService> logger) : BackgroundService
{
    private const string CommandExchange = "mdaresna.platform-events";
    private const string CommandQueue = "schools.database-migrate.v1";
    private const string ResultExchange = "mdaresna.school-events";
    private const string ResultQueue = "platform.school-database-migrated.v1";
    private const string ResultDeadExchange = "mdaresna.school-events.dead";
    private const string ResultDeadQueue = "platform.school-database-migrated.v1.dead";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuration.GetValue<bool>("SchoolProvisioningConsumer:Enabled"))
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            return;
        }
        var configuredUri = configuration["SchoolProvisioningConsumer:BrokerUri"];
        if (!Uri.TryCreate(configuredUri, UriKind.Absolute, out var brokerUri) ||
            (brokerUri.Scheme != "amqps" && !(environment.IsDevelopment() && brokerUri.Scheme == "amqp")))
            throw new InvalidOperationException("SchoolProvisioningConsumer:BrokerUri must use AMQPS (AMQP is allowed only in Development).");
        var factory = new ConnectionFactory { Uri = brokerUri, AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true, ClientProvidedName = "mdaresna-schools:database-migration-consumer" };
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var connection = await factory.CreateConnectionAsync(stoppingToken);
                await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true), stoppingToken);
                await DeclareTopologyAsync(channel, stoppingToken);
                await channel.BasicQosAsync(0, 1, false, stoppingToken);
                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, delivery) => await HandleAsync(channel, delivery, stoppingToken);
                await channel.BasicConsumeAsync(CommandQueue, false, consumer, stoppingToken);
                logger.LogInformation("Schools database migration consumer is subscribed.");
                while (!stoppingToken.IsCancellationRequested && connection.IsOpen && channel.IsOpen)
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Schools database migration consumer is unavailable; reconnecting.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private static async Task DeclareTopologyAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(CommandExchange, ExchangeType.Topic, true, false, null, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(CommandQueue, true, false, false, null, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(CommandQueue, CommandExchange, MigrateSchoolDatabaseV1.MessageType,
            arguments: null, cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(ResultExchange, ExchangeType.Topic, true, false, null, cancellationToken: cancellationToken);
        await channel.ExchangeDeclareAsync(ResultDeadExchange, ExchangeType.Direct, true, false, null, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(ResultDeadQueue, true, false, false,
            new Dictionary<string, object?> { ["x-message-ttl"] = 86_400_000 }, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(ResultDeadQueue, ResultDeadExchange, ResultDeadQueue,
            arguments: null, cancellationToken: cancellationToken);
        var resultArguments = new Dictionary<string, object?> {
            ["x-dead-letter-exchange"] = ResultDeadExchange,
            ["x-dead-letter-routing-key"] = ResultDeadQueue };
        await channel.QueueDeclareAsync(ResultQueue, true, false, false, resultArguments, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(ResultQueue, ResultExchange, SchoolDatabaseMigratedV1.MessageType,
            arguments: null, cancellationToken: cancellationToken);
    }

    private async Task HandleAsync(IChannel channel, BasicDeliverEventArgs delivery,
        CancellationToken cancellationToken)
    {
        IntegrationMessageEnvelope<MigrateSchoolDatabaseV1>? envelope = null;
        try
        {
            if (delivery.Body.Length > 64 * 1024) throw new JsonException("Migration command is too large.");
            envelope = IntegrationJsonSerializer.Deserialize<MigrateSchoolDatabaseV1>(
                Encoding.UTF8.GetString(delivery.Body.Span));
            if (delivery.RoutingKey != MigrateSchoolDatabaseV1.MessageType || envelope.Producer != "mdaresna-platform")
                throw new JsonException("Unexpected migration command source.");
            SchoolDatabaseMigratedV1 result;
            try
            {
                result = await MigrateAsync(envelope.Data, cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                logger.LogError(ex, "School database migration {OperationId} failed.", envelope.Data.OperationId);
                result = new SchoolDatabaseMigratedV1(envelope.Data.OperationId, envelope.Data.TenantId,
                    envelope.Data.SchoolId, false, null, null, LimitError(ex), DateTimeOffset.UtcNow);
            }
            await PublishResultAsync(channel, envelope, result, cancellationToken);
            await channel.BasicAckAsync(delivery.DeliveryTag, false, cancellationToken);
        }
        catch (Exception ex) when (ex is JsonException or ArgumentException)
        {
            logger.LogWarning(ex, "Invalid school database migration command was discarded.");
            await channel.BasicRejectAsync(delivery.DeliveryTag, false, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "School database migration command could not be read; delivery will be retried.");
            await channel.BasicNackAsync(delivery.DeliveryTag, false, true, cancellationToken);
        }
    }

    private async Task<SchoolDatabaseMigratedV1> MigrateAsync(MigrateSchoolDatabaseV1 command,
        CancellationToken cancellationToken)
    {
        if (command.TenantId.Value != command.SchoolId.Value)
            throw new ArgumentException("School and tenant identifiers do not match.");
        var provider = configuration["SchoolProvisioningDatabase:Provider"];
        if (!string.Equals(provider, "PostgreSql", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("SchoolProvisioningDatabase:Provider must be PostgreSql.");
        var adminConnection = configuration["SchoolProvisioningDatabase:AdminConnectionString"];
        if (string.IsNullOrWhiteSpace(adminConnection))
            throw new InvalidOperationException("SchoolProvisioningDatabase:AdminConnectionString is required.");
        var prefix = configuration["SchoolProvisioningDatabase:DatabaseNamePrefix"] ?? "mdaresna_school_";
        var expectedName = $"{prefix}{command.TenantId.Value:N}".ToLowerInvariant();
        if (!string.Equals(command.DatabaseName, expectedName, StringComparison.Ordinal))
            throw new ArgumentException("The migration target does not match the tenant database.");

        var target = new NpgsqlConnectionStringBuilder(adminConnection) { Database = command.DatabaseName };
        await using var db = new PostgreSqlSchoolsDbContext(
            new DbContextOptionsBuilder<PostgreSqlSchoolsDbContext>().UseNpgsql(target.ConnectionString).Options);
        var before = (await db.Database.GetAppliedMigrationsAsync(cancellationToken)).LastOrDefault();
        await db.Database.MigrateAsync(cancellationToken);
        var after = (await db.Database.GetAppliedMigrationsAsync(cancellationToken)).LastOrDefault();
        return new SchoolDatabaseMigratedV1(command.OperationId, command.TenantId, command.SchoolId,
            true, before, after, null, DateTimeOffset.UtcNow);
    }

    private static async Task PublishResultAsync(IChannel channel,
        IntegrationMessageEnvelope<MigrateSchoolDatabaseV1> command,
        SchoolDatabaseMigratedV1 result,
        CancellationToken cancellationToken)
    {
        var envelope = new IntegrationMessageEnvelope<SchoolDatabaseMigratedV1>(
            Guid.NewGuid(), SchoolDatabaseMigratedV1.MessageType, SchoolDatabaseMigratedV1.SchemaVersion,
            result.CompletedAtUtc, "mdaresna-schools", command.Scope, command.Aggregate,
            command.CorrelationId, command.MessageId, command.TraceParent, result);
        var properties = new BasicProperties { Persistent = true, ContentType = "application/json",
            MessageId = envelope.MessageId.ToString("D"), Type = envelope.MessageType,
            CorrelationId = envelope.CorrelationId.ToString("D") };
        await channel.BasicPublishAsync(ResultExchange, SchoolDatabaseMigratedV1.MessageType, true, properties,
            Encoding.UTF8.GetBytes(IntegrationJsonSerializer.Serialize(envelope)), cancellationToken);
    }

    private static string LimitError(Exception exception)
    {
        var message = $"{exception.GetType().Name}: {exception.Message}";
        return message.Length <= 2000 ? message : message[..2000];
    }
}
