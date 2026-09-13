using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Mdaresna.Platform.Infrastructure.Messaging;

/// <summary>
/// Resolves the current Platform provider for each send. A pending audit row
/// is persisted before contacting the gateway, then completed with its raw
/// acknowledgement encrypted at rest. No SMS is sent if audit is unavailable.
/// </summary>
public sealed class PlatformDbSmsSender(
    PlatformDbContext db,
    IConfiguration configuration,
    HttpClient? httpClient = null) : IPlatformSmsSender
{
    public async Task SendAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default)
        => await SendAsync(phoneNumber, message, "other", null, cancellationToken);

    public async Task SendAsync(
        string phoneNumber,
        string message,
        string messageTypeCode,
        Guid? schoolId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(messageTypeCode) ||
            messageTypeCode.Length > 64 ||
            messageTypeCode[0] is < 'a' or > 'z' ||
            messageTypeCode[^1] == '-' ||
            messageTypeCode.Any(ch => ch is not (>= 'a' and <= 'z') and
                not (>= '0' and <= '9') and not '-'))
        {
            throw new ArgumentException("SMS message type must be a lowercase code.", nameof(messageTypeCode));
        }
        if (schoolId == Guid.Empty)
        {
            throw new ArgumentException("SchoolId cannot be empty.", nameof(schoolId));
        }

        // The same deployment-owned key encrypts provider credentials and SMS
        // audit payloads. Without it we cannot safely keep an OTP transcript.
        var secrets = new PlatformSmsSecretProtector(configuration);
        var log = new PlatformSmsLog
        {
            Id = Guid.NewGuid(),
            SourceSystem = "platform",
            MessageTypeCode = messageTypeCode,
            SchoolId = schoolId,
            RecipientEncrypted = secrets.ProtectPayload(phoneNumber),
            RecipientMasked = MaskRecipient(phoneNumber),
            MessageEncrypted = secrets.ProtectPayload(message),
            Status = "Pending",
            CreatedAtUtc = DateTimeOffset.UtcNow
        };
        db.SmsLogs.Add(log);
        await SaveAuditAsync(db, cancellationToken);

        PlatformSmsSendResult result;
        try
        {
            var provider = await db.SmsProviders.AsNoTracking()
                .Where(x => !x.IsDeleted && x.IsActive)
                .OrderBy(x => x.Priority)
                .ThenBy(x => x.CreatedAtUtc)
                .ThenBy(x => x.Id)
                .FirstOrDefaultAsync(cancellationToken);

            PlatformSmsOptions options;
            if (provider is not null)
            {
                // MessageCharactersLength is legacy metadata, not a send cap.
                log.ProviderId = provider.Id;
                options = new PlatformSmsOptions
                {
                    ApiUrlTemplate = provider.ApiUrlTemplate,
                    Username = provider.ProviderUserName,
                    Password = secrets.Unprotect(provider.EncryptedPassword),
                    SenderName = provider.SenderName,
                    SuccessResponsePrefix = provider.SuccessResponsePrefix
                };
                // Keep the provider association even if the process exits
                // while the HTTP request is in flight.
                await SaveAuditAsync(db, cancellationToken);
            }
            else if (!await db.SmsProviders.AsNoTracking().AnyAsync(cancellationToken))
            {
                // This fallback exists only before the first provider row.
                options = new PlatformSmsOptions
                {
                    ApiUrlTemplate = configuration["PlatformSms:ApiUrlTemplate"],
                    Username = configuration["PlatformSms:Username"],
                    Password = configuration["PlatformSms:Password"],
                    SenderName = configuration["PlatformSms:SenderName"],
                    SuccessResponsePrefix = configuration["PlatformSms:SuccessResponsePrefix"]
                };
            }
            else
            {
                throw new PlatformSmsDeliveryException(
                    "No active Platform SMS provider is configured.");
            }

            using var sender = httpClient is null
                ? new PlatformSmsSender(options)
                : new PlatformSmsSender(options, httpClient);
            result = await sender.SendForAuditAsync(
                phoneNumber, message, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            await CompleteFailureAsync(log, "cancelled");
            throw;
        }
        catch (PlatformSmsDeliveryException)
        {
            await CompleteFailureAsync(log, "provider_configuration");
            throw;
        }
        catch (ArgumentException)
        {
            await CompleteFailureAsync(log, "invalid_request");
            throw new PlatformSmsDeliveryException("SMS recipient or message is invalid.");
        }
        catch (Exception)
        {
            await CompleteFailureAsync(log, "provider_configuration");
            throw new PlatformSmsDeliveryException("SMS delivery could not be started.");
        }

        log.HttpStatusCode = result.HttpStatusCode;
        log.ResponseEncrypted = result.ProviderResponse is null
            ? null
            : secrets.ProtectPayload(result.ProviderResponse);
        log.Status = result.Accepted
            ? "Accepted"
            : result.FailureReason == "provider_unavailable" ? "Failed" : "Rejected";
        log.FailureReason = result.FailureReason;
        log.CompletedAtUtc = DateTimeOffset.UtcNow;
        await SaveAuditAsync(db, CancellationToken.None);

        if (!result.Accepted)
        {
            throw new PlatformSmsDeliveryException(result.FailureMessage!);
        }
    }

    private async Task CompleteFailureAsync(PlatformSmsLog log, string reason)
    {
        log.Status = "Failed";
        log.FailureReason = reason;
        log.CompletedAtUtc = DateTimeOffset.UtcNow;
        await SaveAuditAsync(db, CancellationToken.None);
    }

    private static async Task SaveAuditAsync(
        PlatformDbContext db,
        CancellationToken cancellationToken)
    {
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception)
        {
            // No DB/provider payload or connection-string detail is exposed.
            throw new PlatformSmsDeliveryException("SMS audit log is unavailable.");
        }
    }

    private static string MaskRecipient(string phoneNumber)
    {
        var normalized = phoneNumber.Trim();
        return normalized.Length <= 4
            ? new string('*', normalized.Length)
            : new string('*', normalized.Length - 4) + normalized[^4..];
    }
}
