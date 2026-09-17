using System.Text;
using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;

namespace Mdaresna.Platform.Worker;

internal sealed class PlatformOutboxPublisherService(
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<PlatformOutboxPublisherService> logger) : BackgroundService
{
    internal const string Exchange = "mdaresna.platform-events";
    internal const string ProvisionQueue = "schools.provision-school.v1";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuration.GetValue<bool>("PlatformOutboxPublisher:Enabled"))
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            return;
        }

        var configuredUri = configuration["PlatformOutboxPublisher:BrokerUri"];
        if (!Uri.TryCreate(configuredUri, UriKind.Absolute, out var brokerUri) ||
            (brokerUri.Scheme != "amqps" && !(environment.IsDevelopment() && brokerUri.Scheme == "amqp")))
            throw new InvalidOperationException("PlatformOutboxPublisher:BrokerUri must use AMQPS (AMQP is allowed only in Development).");

        var factory = new ConnectionFactory { Uri = brokerUri, AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true, ClientProvidedName = "mdaresna-platform:outbox-publisher" };
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var connection = await factory.CreateConnectionAsync(stoppingToken);
                await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true), stoppingToken);
                await DeclareTopologyAsync(channel, stoppingToken);
                while (!stoppingToken.IsCancellationRequested && connection.IsOpen && channel.IsOpen)
                {
                    var published = await PublishBatchAsync(channel, stoppingToken);
                    if (!published) await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "Platform outbox publisher is unavailable; reconnecting.");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }

    private static async Task DeclareTopologyAsync(IChannel channel, CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, durable: true, autoDelete: false,
            arguments: null, cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(ProvisionQueue, durable: true, exclusive: false, autoDelete: false,
            arguments: null, cancellationToken: cancellationToken);
        await channel.QueueBindAsync(ProvisionQueue, Exchange, ProvisionSchoolV1.MessageType,
            arguments: null, cancellationToken: cancellationToken);
    }

    private async Task<bool> PublishBatchAsync(IChannel channel, CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var now = DateTimeOffset.UtcNow;
        var messages = await db.OutboxMessages
            .Where(x => x.ProcessedAtUtc == null && (x.NextAttemptAtUtc == null || x.NextAttemptAtUtc <= now))
            .OrderBy(x => x.OccurredAtUtc).Take(50).ToArrayAsync(cancellationToken);
        foreach (var message in messages)
        {
            try
            {
                var properties = new BasicProperties { Persistent = true, ContentType = "application/json",
                    MessageId = message.Id.ToString("D"), Type = message.MessageType,
                    CorrelationId = message.CorrelationId.ToString("D") };
                await channel.BasicPublishAsync(Exchange, message.MessageType, mandatory: true, properties,
                    Encoding.UTF8.GetBytes(message.PayloadJson), cancellationToken);
                message.ProcessedAtUtc = DateTimeOffset.UtcNow;
                message.LastError = null;
                message.NextAttemptAtUtc = null;
            }
            catch (Exception ex)
            {
                message.AttemptCount++;
                message.LastError = ex.GetType().Name;
                message.NextAttemptAtUtc = DateTimeOffset.UtcNow.AddSeconds(Math.Min(300, Math.Pow(2, message.AttemptCount)));
            }
        }
        if (messages.Length > 0) await db.SaveChangesAsync(cancellationToken);
        return messages.Length > 0;
    }
}
