using System.Text;
using System.Text.Json;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.Platform.Contracts.Messaging;
using Mdaresna.Platform.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Mdaresna.Platform.Worker;

internal sealed class PlatformWorkerService(
    WorkerReadinessState readinessState,
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    ILogger<PlatformWorkerService> logger) : BackgroundService
{
    private const string Exchange = "mdaresna.sms-audit";
    private const string DeadExchange = "mdaresna.sms-audit.dead";
    private const string Queue = "platform.sms-log.v1";
    private const string DeadQueue = "platform.sms-log.v1.dead";
    private const string ReadinessComponent = "sms-log";
    private const int MaximumEventBytes = 64 * 1024;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuration.GetValue<bool>("SmsLogConsumer:Enabled"))
        {
            logger.LogInformation("Platform worker is running with SMS event consumption disabled.");
            try
            {
                await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                // Normal shutdown.
            }
            return;
        }

        var configuredUri = configuration["SmsLogConsumer:BrokerUri"];
        if (!Uri.TryCreate(configuredUri, UriKind.Absolute, out var brokerUri) ||
            brokerUri.Scheme != "amqps" || string.IsNullOrWhiteSpace(brokerUri.Host))
        {
            throw new InvalidOperationException(
                "SmsLogConsumer:BrokerUri must be an AMQPS URI from the deployment secret store.");
        }

        var factory = new ConnectionFactory
        {
            Uri = brokerUri,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            ClientProvidedName = "mdaresna-platform:sms-log-consumer"
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var connection = await factory.CreateConnectionAsync(stoppingToken);
                await using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);
                await DeclareTopologyAsync(channel, stoppingToken);
                await channel.BasicQosAsync(0, 1, false, stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, delivery) =>
                    await HandleDeliveryAsync(channel, delivery, stoppingToken);
                await channel.BasicConsumeAsync(Queue, autoAck: false, consumer, stoppingToken);

                logger.LogInformation("Platform SMS audit consumer is subscribed.");
                while (!stoppingToken.IsCancellationRequested &&
                       connection.IsOpen && channel.IsOpen)
                {
                    readinessState.MarkReady(ReadinessComponent);
                    await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception)
            {
                // Never log the broker URI or delivery body. The URI contains
                // credentials and the event may contain an OTP.
                logger.LogWarning("Platform SMS audit consumer is unavailable; reconnecting.");
            }
            finally
            {
                readinessState.MarkNotReady(ReadinessComponent);
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }

    private static async Task DeclareTopologyAsync(IChannel channel, CancellationToken ct)
    {
        await channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic,
            durable: true, autoDelete: false, arguments: null, cancellationToken: ct);
        await channel.ExchangeDeclareAsync(DeadExchange, ExchangeType.Direct,
            durable: true, autoDelete: false, arguments: null, cancellationToken: ct);
        // Invalid payloads may contain OTPs. Keep them briefly for operator
        // diagnosis, rather than retaining plaintext indefinitely in the DLQ.
        var deadArguments = new Dictionary<string, object?>
        {
            ["x-message-ttl"] = 3_600_000
        };
        await channel.QueueDeclareAsync(DeadQueue, durable: true, exclusive: false,
            autoDelete: false, arguments: deadArguments, cancellationToken: ct);
        await channel.QueueBindAsync(DeadQueue, DeadExchange, DeadQueue,
            arguments: null, cancellationToken: ct);

        var arguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = DeadExchange,
            ["x-dead-letter-routing-key"] = DeadQueue
        };
        await channel.QueueDeclareAsync(Queue, durable: true, exclusive: false,
            autoDelete: false, arguments: arguments, cancellationToken: ct);
        await channel.QueueBindAsync(Queue, Exchange, "sms.delivery.schools",
            arguments: null, cancellationToken: ct);
        await channel.QueueBindAsync(Queue, Exchange, "sms.delivery.family",
            arguments: null, cancellationToken: ct);
    }

    private async Task HandleDeliveryAsync(
        IChannel channel,
        BasicDeliverEventArgs delivery,
        CancellationToken stoppingToken)
    {
        try
        {
            if (delivery.Body.Length > MaximumEventBytes)
            {
                throw new JsonException("SMS audit event is too large.");
            }

            // RabbitMQ owns delivery memory after this callback; deserialize
            // immediately and never retain or log the raw body.
            var json = Encoding.UTF8.GetString(delivery.Body.Span);
            var envelope = IntegrationJsonSerializer.Deserialize<SmsDeliveryAttemptRecordedV1>(json);
            if (envelope.Producer is not ("schools" or "family") ||
                delivery.RoutingKey != $"sms.delivery.{envelope.Producer}")
            {
                throw new JsonException("SMS audit event source does not match routing key.");
            }

            await using var scope = scopeFactory.CreateAsyncScope();
            var ingestor = scope.ServiceProvider.GetRequiredService<PlatformSmsLogEventIngestor>();
            await ingestor.IngestAsync(envelope, stoppingToken);
            await channel.BasicAckAsync(delivery.DeliveryTag, multiple: false, stoppingToken);
        }
        catch (Exception exception) when (exception is JsonException or ArgumentException)
        {
            logger.LogWarning("Invalid SMS audit event moved to the dead-letter queue.");
            await channel.BasicRejectAsync(delivery.DeliveryTag, requeue: false, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Closing the channel requeues an unacknowledged delivery.
        }
        catch (Exception)
        {
            logger.LogWarning("SMS audit event persistence failed; delivery will be retried.");
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            await channel.BasicNackAsync(delivery.DeliveryTag, multiple: false,
                requeue: true, stoppingToken);
        }
    }
}
