using System.Text;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.Schools.Application.Registration;
using Mdaresna.Schools.Contracts.Registration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;

namespace Mdaresna.Schools.Infrastructure.Messaging;

public sealed class RabbitMqSchoolRegistrationRequestPublisher(
    IConfiguration configuration,
    IHostEnvironment environment)
    : ISchoolRegistrationRequestPublisher
{
    public const string Exchange = "mdaresna.school-registry";
    public const string RoutingKey = "school.registration.requested.v2";

    public async Task PublishAsync(IntegrationMessageEnvelope<SchoolRegistrationRequestedV2> envelope,
        CancellationToken cancellationToken = default)
    {
        var configuredUri = configuration["SchoolRegistrationPublisher:BrokerUri"];
        if (!Uri.TryCreate(configuredUri, UriKind.Absolute, out var brokerUri) ||
            (brokerUri.Scheme != "amqps" && !(environment.IsDevelopment() && brokerUri.Scheme == "amqp")) ||
            string.IsNullOrWhiteSpace(brokerUri.Host))
            throw new InvalidOperationException(
                "SchoolRegistrationPublisher:BrokerUri must use AMQPS (AMQP is allowed only in Development)." );

        var factory = new ConnectionFactory
        {
            Uri = brokerUri,
            AutomaticRecoveryEnabled = true,
            TopologyRecoveryEnabled = true,
            ClientProvidedName = "mdaresna-schools:school-registration-publisher"
        };
        await using var connection = await factory.CreateConnectionAsync(cancellationToken);
        await using var channel = await connection.CreateChannelAsync(
            new CreateChannelOptions(true, true), cancellationToken);
        await channel.ExchangeDeclareAsync(Exchange, ExchangeType.Topic, durable: true,
            autoDelete: false, arguments: null, cancellationToken: cancellationToken);
        var body = Encoding.UTF8.GetBytes(IntegrationJsonSerializer.Serialize(envelope));
        var properties = new BasicProperties
        {
            Persistent = true,
            ContentType = "application/json",
            MessageId = envelope.MessageId.ToString("D"),
            Type = envelope.MessageType,
            CorrelationId = envelope.CorrelationId.ToString("D")
        };
        await channel.BasicPublishAsync(Exchange, RoutingKey, mandatory: true,
            properties, body, cancellationToken);
    }
}
