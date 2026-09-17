using System.Text;
using System.Text.Json;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.Schools.Infrastructure.Persistence;
using Mdaresna.Schools.Domain.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Mdaresna.Schools.Infrastructure.Identity;
using Npgsql;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Mdaresna.Schools.Infrastructure.Messaging;

public sealed class SchoolProvisioningConsumerService(
    IConfiguration configuration,
    IHostEnvironment environment,
    IServiceScopeFactory scopeFactory,
    ILogger<SchoolProvisioningConsumerService> logger) : BackgroundService
{
    private const string Exchange = "mdaresna.platform-events";
    private const string Queue = "schools.provision-school.v1";
    private const string DeadExchange = "mdaresna.platform-events.dead";
    private const string DeadQueue = "schools.provision-school.v1.dead";
    private const string ResultDeadExchange = "mdaresna.school-events.dead";
    private const string ResultDeadQueue = "platform.school-provisioned.v1.dead";
    internal const string ResultExchange = "mdaresna.school-events";
    internal const string ResultQueue = "platform.school-provisioned.v1";

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
            TopologyRecoveryEnabled = true, ClientProvidedName = "mdaresna-schools:provisioning-consumer" };
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
                await channel.BasicConsumeAsync(Queue, autoAck: false, consumer, stoppingToken);
                logger.LogInformation("Schools provisioning consumer is subscribed.");
                while (!stoppingToken.IsCancellationRequested && connection.IsOpen && channel.IsOpen)
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Schools provisioning consumer is unavailable; reconnecting.");
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
        // This queue is also declared by the Platform outbox publisher. Keep its declaration
        // argument-free on both sides; poison messages are explicitly copied to the dead queue.
        await channel.QueueDeclareAsync(Queue, true, false, false, null, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(Queue, Exchange, ProvisionSchoolV1.MessageType, arguments: null, cancellationToken: cancellationToken);
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
        await channel.QueueBindAsync(ResultQueue, ResultExchange, SchoolProvisionedV1.MessageType, arguments: null, cancellationToken: cancellationToken);
    }

    private async Task HandleAsync(IChannel channel, BasicDeliverEventArgs delivery, CancellationToken cancellationToken)
    {
        try
        {
            if (delivery.Body.Length > 128 * 1024) throw new JsonException("Provisioning command is too large.");
            var envelope = IntegrationJsonSerializer.Deserialize<ProvisionSchoolV1>(Encoding.UTF8.GetString(delivery.Body.Span));
            if (delivery.RoutingKey != ProvisionSchoolV1.MessageType || envelope.Producer != "mdaresna-platform")
                throw new JsonException("Unexpected provisioning command source.");
            var target = await ProvisionPostgreSqlAsync(envelope.Data, cancellationToken);
            var completedAtUtc = target.ActivatedAtUtc;
            var result = new SchoolProvisionedV1(envelope.Data.OperationId, envelope.Data.TenantId,
                envelope.Data.SchoolId, target.LocalSchoolId, "PostgreSql", target.Host, target.Port, target.DatabaseName,
                target.CredentialSecretReference, target.RequireTls, target.DatabaseSchemaVersion, completedAtUtc,
                target.OwnerFullUserName);
            var resultEnvelope = new IntegrationMessageEnvelope<SchoolProvisionedV1>(
                envelope.Data.OperationId, SchoolProvisionedV1.MessageType, SchoolProvisionedV1.SchemaVersion,
                completedAtUtc, "mdaresna-schools", envelope.Scope, envelope.Aggregate,
                envelope.CorrelationId, envelope.MessageId, envelope.TraceParent, result);
            var properties = new BasicProperties { Persistent = true, ContentType = "application/json",
                MessageId = resultEnvelope.MessageId.ToString("D"), Type = resultEnvelope.MessageType,
                CorrelationId = resultEnvelope.CorrelationId.ToString("D") };
            await channel.BasicPublishAsync(ResultExchange, SchoolProvisionedV1.MessageType, true, properties,
                Encoding.UTF8.GetBytes(IntegrationJsonSerializer.Serialize(resultEnvelope)), cancellationToken);
            await channel.BasicAckAsync(delivery.DeliveryTag, false, cancellationToken);
            logger.LogInformation("School database provisioned for tenant {TenantId}; operation {OperationId}.",
                envelope.Data.TenantId, envelope.Data.OperationId);
        }
        catch (Exception ex) when (ex is JsonException or ArgumentException)
        {
            logger.LogWarning("Invalid school provisioning command moved to the dead-letter queue.");
            var properties = new BasicProperties { Persistent = true, ContentType = delivery.BasicProperties.ContentType,
                MessageId = delivery.BasicProperties.MessageId, Type = delivery.BasicProperties.Type,
                CorrelationId = delivery.BasicProperties.CorrelationId };
            await channel.BasicPublishAsync(DeadExchange, DeadQueue, true, properties,
                delivery.Body, cancellationToken);
            await channel.BasicAckAsync(delivery.DeliveryTag, false, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "School database provisioning failed; the command will be retried.");
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
            await channel.BasicNackAsync(delivery.DeliveryTag, false, true, cancellationToken);
        }
    }

    private async Task<ProvisionedTarget> ProvisionPostgreSqlAsync(ProvisionSchoolV1 command, CancellationToken cancellationToken)
    {
        var provider = configuration["SchoolProvisioningDatabase:Provider"];
        if (!string.Equals(provider, "PostgreSql", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("SchoolProvisioningDatabase:Provider must be PostgreSql.");
        var adminConnection = configuration["SchoolProvisioningDatabase:AdminConnectionString"];
        if (string.IsNullOrWhiteSpace(adminConnection))
            throw new InvalidOperationException("SchoolProvisioningDatabase:AdminConnectionString is required.");
        var prefix = configuration["SchoolProvisioningDatabase:DatabaseNamePrefix"] ?? "mdaresna_school_";
        if (!prefix.All(character => char.IsAsciiLetterOrDigit(character) || character == '_'))
            throw new InvalidOperationException("School database prefix contains invalid characters.");
        var databaseName = $"{prefix}{command.TenantId.Value:N}".ToLowerInvariant();

        await using (var admin = new NpgsqlConnection(adminConnection))
        {
            await admin.OpenAsync(cancellationToken);
            await using var exists = new NpgsqlCommand("SELECT 1 FROM pg_database WHERE datname = @name", admin);
            exists.Parameters.AddWithValue("name", databaseName);
            if (await exists.ExecuteScalarAsync(cancellationToken) is null)
            {
                await using var create = new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", admin);
                await create.ExecuteNonQueryAsync(cancellationToken);
            }
        }

        var target = new NpgsqlConnectionStringBuilder(adminConnection) { Database = databaseName };
        await using var db = new PostgreSqlSchoolsDbContext(
            new DbContextOptionsBuilder<PostgreSqlSchoolsDbContext>().UseNpgsql(target.ConnectionString).Options);
        await db.Database.MigrateAsync(cancellationToken);
        var databaseSchemaVersion = (await db.Database.GetAppliedMigrationsAsync(cancellationToken))
            .LastOrDefault() ?? "none";
        var local = await db.SchoolInformation.SingleOrDefaultAsync(
            item => item.PlatformSchoolReferenceId == command.SchoolId.Value, cancellationToken);
        var activatedAtUtc = DateTimeOffset.UtcNow;
        if (local is null)
        {
            local = SchoolInformation.Create(command.SchoolId.Value, command.TenantId.Value,
                command.RegistrationRequestId, command.SchoolCode, command.DisplayName, command.SchoolType,
                command.DeploymentMode, command.Address, command.PrimaryPhone, command.UnitTypeId,
                command.UnitTypeCode, command.UnitTypeName, command.UnitPrice, command.Currency,
                command.OwnerPlatformAccountId, command.PlatformCreatedAtUtc, activatedAtUtc);
            db.SchoolInformation.Add(local);
            await db.SaveChangesAsync(cancellationToken);
        }
        else
        {
            activatedAtUtc = local.ActivatedAtUtc;
        }
        await using var scope = scopeFactory.CreateAsyncScope();
        var owner = await scope.ServiceProvider.GetRequiredService<ISchoolIdentityBootstrapper>()
            .BootstrapOwnerAsync(db, command.OwnerPlatformAccountId, cancellationToken);
        var secretReference = configuration["SchoolProvisioningDatabase:CredentialSecretReference"];
        if (string.IsNullOrWhiteSpace(secretReference))
            throw new InvalidOperationException("SchoolProvisioningDatabase:CredentialSecretReference is required.");
        return new ProvisionedTarget(target.Host ?? throw new InvalidOperationException("PostgreSQL host is required."),
            target.Port, databaseName, secretReference,
            target.SslMode is SslMode.Require or SslMode.VerifyCA or SslMode.VerifyFull,
            local.Id, activatedAtUtc, $"{owner.UserName}@{command.SchoolCode}", databaseSchemaVersion);
    }

    private sealed record ProvisionedTarget(string Host, int Port, string DatabaseName,
        string CredentialSecretReference, bool RequireTls, Guid LocalSchoolId, DateTimeOffset ActivatedAtUtc,
        string OwnerFullUserName, string DatabaseSchemaVersion);
}
