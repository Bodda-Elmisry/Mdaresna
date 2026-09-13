namespace Mdaresna.Platform.Infrastructure.Messaging;

/// <summary>
/// The existing SMS provider accepts a URL template with five positional fields:
/// username, password, sender, recipient and message. Supply secrets through
/// environment-specific configuration or a secret store, never source control.
/// </summary>
public sealed class PlatformSmsOptions
{
    public const string SectionName = "PlatformSms";
    public const string LegacyNonEmptyResponse = "legacy:any-nonempty";

    public string? ApiUrlTemplate { get; init; }

    public string? Username { get; init; }

    public string? Password { get; init; }

    public string? SenderName { get; init; }

    /// <summary>
    /// Required response prefix that unequivocally means the provider accepted
    /// the message. HTTP 200 alone is not proof of provider acceptance.
    /// </summary>
    public string? SuccessResponsePrefix { get; init; }
}
