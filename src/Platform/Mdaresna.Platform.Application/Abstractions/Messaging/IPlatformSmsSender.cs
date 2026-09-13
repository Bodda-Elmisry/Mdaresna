namespace Mdaresna.Platform.Application.Abstractions.Messaging;

/// <summary>
/// Sends a short transactional message to a verified or pending phone number.
/// The caller owns the challenge lifecycle. Platform audit stores SMS payloads
/// as authenticated ciphertext, never as readable OTP text.
/// </summary>
public interface IPlatformSmsSender
{
    Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);

    Task SendAsync(
        string phoneNumber,
        string message,
        string messageTypeCode,
        Guid? schoolId,
        CancellationToken cancellationToken = default) =>
        SendAsync(phoneNumber, message, cancellationToken);
}
