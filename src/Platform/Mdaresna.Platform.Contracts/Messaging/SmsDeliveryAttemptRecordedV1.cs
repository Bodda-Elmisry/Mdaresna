using System.Text;
using System.Text.Json.Serialization;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Contracts.Common;

namespace Mdaresna.Platform.Contracts.Messaging;

/// <summary>
/// One completed SMS attempt from Schools or Family to the Platform log.
/// The envelope's MessageId is the durable source-attempt idempotency key;
/// Producer identifies the source system and Scope identifies the school.
/// The payload is sensitive and must travel only over a protected broker.
/// </summary>
public sealed record SmsDeliveryAttemptRecordedV1 : IIntegrationEvent
{
    [JsonConstructor]
    public SmsDeliveryAttemptRecordedV1(
        string messageTypeCode,
        string recipient,
        string message,
        string? providerResponse,
        int? httpStatusCode,
        string outcome,
        string? failureReason,
        DateTimeOffset attemptedAtUtc,
        DateTimeOffset completedAtUtc)
    {
        MessageTypeCode = Code(messageTypeCode, 64, nameof(messageTypeCode));
        Recipient = ContractGuard.Text(recipient, 20, nameof(recipient));
        if (Recipient.Length < 8 ||
            Recipient.Skip(Recipient[0] == '+' ? 1 : 0).Any(ch => ch is < '0' or > '9'))
        {
            throw new ArgumentException("Recipient must be a phone number.", nameof(recipient));
        }
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        if (message.Length > 1000)
        {
            throw new ArgumentOutOfRangeException(nameof(message));
        }
        Message = message;
        if (providerResponse is not null && Encoding.UTF8.GetByteCount(providerResponse) > 16 * 1024)
        {
            throw new ArgumentOutOfRangeException(nameof(providerResponse));
        }
        ProviderResponse = providerResponse;
        if (httpStatusCode is < 100 or > 599)
        {
            throw new ArgumentOutOfRangeException(nameof(httpStatusCode));
        }
        HttpStatusCode = httpStatusCode;
        Outcome = outcome is "Accepted" or "Rejected" or "Failed"
            ? outcome
            : throw new ArgumentException("Unknown SMS outcome.", nameof(outcome));
        FailureReason = failureReason is null
            ? null
            : Code(failureReason, 256, nameof(failureReason), allowUnderscore: true);
        if (Outcome == "Accepted" && FailureReason is not null)
        {
            throw new ArgumentException("Accepted SMS cannot have a failure reason.", nameof(failureReason));
        }
        AttemptedAtUtc = ContractGuard.Utc(attemptedAtUtc, nameof(attemptedAtUtc));
        CompletedAtUtc = ContractGuard.Utc(completedAtUtc, nameof(completedAtUtc));
        if (CompletedAtUtc < AttemptedAtUtc)
        {
            throw new ArgumentException("Completion cannot precede attempt.", nameof(completedAtUtc));
        }
    }

    public static string MessageType => "mdaresna.messaging.sms-delivery-attempt-recorded";
    public static ushort SchemaVersion => 1;

    [JsonPropertyName("messageTypeCode")]
    public string MessageTypeCode { get; }

    [JsonPropertyName("recipient")]
    public string Recipient { get; }

    [JsonPropertyName("message")]
    public string Message { get; }

    [JsonPropertyName("providerResponse")]
    public string? ProviderResponse { get; }

    [JsonPropertyName("httpStatusCode")]
    public int? HttpStatusCode { get; }

    [JsonPropertyName("outcome")]
    public string Outcome { get; }

    [JsonPropertyName("failureReason")]
    public string? FailureReason { get; }

    [JsonPropertyName("attemptedAtUtc")]
    public DateTimeOffset AttemptedAtUtc { get; }

    [JsonPropertyName("completedAtUtc")]
    public DateTimeOffset CompletedAtUtc { get; }

    private static string Code(
        string? value, int maxLength, string name, bool allowUnderscore = false)
    {
        var code = ContractGuard.Text(value, maxLength, name);
        if (code[0] is < 'a' or > 'z' || code[^1] == '-' ||
            code.Any(ch => !(ch is >= 'a' and <= 'z' or >= '0' and <= '9' or '-' ||
                             allowUnderscore && ch == '_')))
        {
            throw new ArgumentException("Value must be a lowercase code.", name);
        }
        return code;
    }
}
