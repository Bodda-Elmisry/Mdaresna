using System.Text;
using System.Text.Json;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Schools.Contracts.Registration;
using Mdaresna.Platform.Infrastructure.Messaging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Mdaresna.Platform.Worker;

internal sealed class SchoolRegistrationRequestConsumerService(
    WorkerReadinessState readinessState,
    IServiceScopeFactory scopeFactory,
    IConfiguration configuration,
    IHostEnvironment environment,
    ILogger<SchoolRegistrationRequestConsumerService> logger) : BackgroundService
{
    public const string Exchange = "mdaresna.school-registry";
    public const string RoutingKey = "school.registration.requested.v2";
    public const string Queue = "platform.school-registration-requested.v2";
    private const string DeadExchange = "mdaresna.school-registry.dead";
    private const string DeadQueue = "platform.school-registration-requested.v2.dead";
    private const string ReadinessComponent = "school-registration";
    private const int MaximumEventBytes = 64 * 1024;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!configuration.GetValue<bool>("SchoolRegistrationConsumer:Enabled"))
        {
            logger.LogInformation(
                "Platform worker is running with school registration consumption disabled.");
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

        var configuredUri = configuration["SchoolRegistrationConsumer:BrokerUri"];
        if (!Uri.TryCreate(configuredUri, UriKind.Absolute, out var brokerUri) ||
            (brokerUri.Scheme != "amqps" && !(environment.IsDevelopment() && brokerUri.Scheme == "amqp")) ||
            string.IsNullOrWhiteSpace(brokerUri.Host))
        {
            throw new InvalidOperationException(
                "SchoolRegistrationConsumer:BrokerUri must use AMQPS (AMQP is allowed only in Development)." );
        }

        var factory = new ConnectionFactory
        {
            Uri = brokerUri,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            ClientProvidedName = "mdaresna-platform:school-registration-consumer"
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var connection = await factory.CreateConnectionAsync(stoppingToken);
                await using var channel = await connection.CreateChannelAsync(
                    cancellationToken: stoppingToken);
                await DeclareTopologyAsync(channel, stoppingToken);
                await channel.BasicQosAsync(0, 1, false, stoppingToken);

                var consumer = new AsyncEventingBasicConsumer(channel);
                consumer.ReceivedAsync += async (_, delivery) =>
                    await HandleDeliveryAsync(channel, delivery, stoppingToken);
                await channel.BasicConsumeAsync(
                    Queue, autoAck: false, consumer, stoppingToken);

                logger.LogInformation(
                    "Platform school registration request consumer is subscribed.");
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
                // Never log the broker URI or event body. The URI contains
                // credentials and the request contains personal data.
                logger.LogWarning(
                    "Platform school registration consumer is unavailable; reconnecting.");
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
        await channel.ExchangeDeclareAsync(
            Exchange, ExchangeType.Topic, durable: true, autoDelete: false,
            arguments: null, cancellationToken: ct);
        await channel.ExchangeDeclareAsync(
            DeadExchange, ExchangeType.Direct, durable: true, autoDelete: false,
            arguments: null, cancellationToken: ct);

        var deadArguments = new Dictionary<string, object?>
        {
            ["x-message-ttl"] = 3_600_000
        };
        await channel.QueueDeclareAsync(
            DeadQueue, durable: true, exclusive: false, autoDelete: false,
            arguments: deadArguments, cancellationToken: ct);
        await channel.QueueBindAsync(
            DeadQueue, DeadExchange, DeadQueue,
            arguments: null, cancellationToken: ct);

        var arguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = DeadExchange,
            ["x-dead-letter-routing-key"] = DeadQueue
        };
        await channel.QueueDeclareAsync(
            Queue, durable: true, exclusive: false, autoDelete: false,
            arguments: arguments, cancellationToken: ct);
        await channel.QueueBindAsync(
            Queue, Exchange, RoutingKey,
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
                throw new JsonException("School registration event is too large.");
            }

            if (!string.Equals(delivery.RoutingKey, RoutingKey, StringComparison.Ordinal))
            {
                throw new JsonException("Unexpected school registration routing key.");
            }

            var json = Encoding.UTF8.GetString(delivery.Body.Span);
            var envelope = IntegrationJsonSerializer
                .Deserialize<SchoolRegistrationRequestedV2>(json);
            if (!string.Equals(envelope.Producer, "schools", StringComparison.Ordinal))
            {
                throw new JsonException("Unexpected school registration producer.");
            }

            await using var scope = scopeFactory.CreateAsyncScope();
            var ingestor = scope.ServiceProvider
                .GetRequiredService<PlatformSchoolRegistrationRequestV2Ingestor>();
            await ingestor.IngestAsync(envelope, stoppingToken);
            await channel.BasicAckAsync(
                delivery.DeliveryTag, multiple: false, stoppingToken);
        }
        catch (Exception exception) when (
            exception is JsonException or ArgumentException or PlatformApplicationException)
        {
            logger.LogWarning(
                "Invalid school registration event moved to the dead-letter queue.");
            await channel.BasicRejectAsync(
                delivery.DeliveryTag, requeue: false, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Closing the channel requeues an unacknowledged delivery.
        }
        catch (Exception)
        {
            logger.LogWarning(
                "School registration event processing failed; delivery will be retried.");
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            await channel.BasicNackAsync(
                delivery.DeliveryTag, multiple: false, requeue: true, stoppingToken);
        }
    }
}
