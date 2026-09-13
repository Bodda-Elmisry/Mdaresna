using System.Globalization;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Mdaresna.Platform.Application.Abstractions.Messaging;

namespace Mdaresna.Platform.Infrastructure.Messaging;

/// <summary>
/// A Platform-owned adapter for the legacy provider's HTTP URL-template protocol.
/// No legacy database, SMS-provider entity or service dependency is used.
/// </summary>
public sealed partial class PlatformSmsSender : IPlatformSmsSender, IDisposable
{
    private static readonly TimeSpan RequestTimeout = TimeSpan.FromSeconds(8);
    private const int MaximumResponseBytes = 16 * 1024;

    private readonly PlatformSmsOptions _options;
    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;

    public PlatformSmsSender(PlatformSmsOptions options)
        : this(options, CreateHttpClient(), ownsHttpClient: true)
    {
    }

    public PlatformSmsSender(PlatformSmsOptions options, HttpClient httpClient)
        : this(options, httpClient, ownsHttpClient: false)
    {
    }

    private PlatformSmsSender(
        PlatformSmsOptions options,
        HttpClient httpClient,
        bool ownsHttpClient)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _ownsHttpClient = ownsHttpClient;
    }

    public async Task SendAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        var result = await SendForAuditAsync(phoneNumber, message, cancellationToken);
        if (!result.Accepted)
        {
            throw new PlatformSmsDeliveryException(result.FailureMessage!);
        }
    }

    /// <summary>
    /// Returns the gateway's acknowledgement only to the Platform persistence
    /// adapter, which encrypts it before storage. Never include it in a public
    /// exception or diagnostic log: it may echo the phone, OTP or credentials.
    /// </summary>
    internal async Task<PlatformSmsSendResult> SendForAuditAsync(
        string phoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        // Do not put the URI or provider response in an exception: the request URI
        // contains credentials, the phone number and the one-time code.
        var requestUri = BuildRequestUri(phoneNumber, message);
        try
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            using var response = await _httpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken);

            // The old provider returns a small textual acknowledgement. Do not
            // expose or log its body, which is not trusted and may echo the OTP.
            // Bound the read in case a gateway returns an unexpectedly large page.
            var (body, truncated) = await ReadResponseAsync(
                response.Content, cancellationToken);
            if (truncated)
            {
                return new PlatformSmsSendResult(false, (int)response.StatusCode,
                    body, "provider_response_too_large",
                    "SMS provider returned an oversized response.");
            }
            if (!response.IsSuccessStatusCode)
            {
                return new PlatformSmsSendResult(false, (int)response.StatusCode,
                    body, "provider_http_rejected",
                    "SMS provider rejected the request.");
            }

            var accepted = _options.SuccessResponsePrefix ==
                PlatformSmsOptions.LegacyNonEmptyResponse
                ? !string.IsNullOrWhiteSpace(body) &&
                  !body.StartsWith("SOMETHING WENT AWRY", StringComparison.OrdinalIgnoreCase)
                : body.TrimStart().StartsWith(
                    _options.SuccessResponsePrefix!,
                    StringComparison.OrdinalIgnoreCase);
            if (!accepted)
            {
                return new PlatformSmsSendResult(false, (int)response.StatusCode,
                    body, "provider_unconfirmed",
                    "SMS provider did not confirm delivery.");
            }
            return new PlatformSmsSendResult(true, (int)response.StatusCode,
                body, null, null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or IOException)
        {
            return new PlatformSmsSendResult(false, null, null,
                "provider_unavailable", "SMS provider is unavailable.");
        }
    }

    private static async Task<(string Body, bool Truncated)> ReadResponseAsync(
        HttpContent content,
        CancellationToken cancellationToken)
    {
        await using var stream = await content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream();
        var chunk = new byte[4096];
        while (true)
        {
            var read = await stream.ReadAsync(chunk, cancellationToken);
            if (read == 0)
            {
                return (Encoding.UTF8.GetString(buffer.ToArray()), false);
            }

            var remaining = MaximumResponseBytes - (int)buffer.Length;
            if (read > remaining)
            {
                if (remaining > 0)
                {
                    buffer.Write(chunk, 0, remaining);
                }
                return (Encoding.UTF8.GetString(buffer.ToArray()), true);
            }
            buffer.Write(chunk, 0, read);
        }
    }

    public void Dispose()
    {
        if (_ownsHttpClient)
        {
            _httpClient.Dispose();
        }
    }

    private Uri BuildRequestUri(string phoneNumber, string message)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiUrlTemplate) ||
            string.IsNullOrWhiteSpace(_options.Username) ||
            string.IsNullOrWhiteSpace(_options.Password) ||
            string.IsNullOrWhiteSpace(_options.SenderName) ||
            string.IsNullOrWhiteSpace(_options.SuccessResponsePrefix))
        {
            throw new PlatformSmsDeliveryException("SMS provider is not configured.");
        }

        if (string.IsNullOrWhiteSpace(phoneNumber) ||
            !PhoneNumberPattern().IsMatch(phoneNumber) ||
            string.IsNullOrWhiteSpace(message) ||
            message.Length > 1000)
        {
            throw new ArgumentException("SMS recipient or message is invalid.");
        }

        string url;
        try
        {
            url = string.Format(
                CultureInfo.InvariantCulture,
                _options.ApiUrlTemplate,
                Uri.EscapeDataString(_options.Username),
                Uri.EscapeDataString(_options.Password),
                Uri.EscapeDataString(_options.SenderName),
                Uri.EscapeDataString(phoneNumber),
                Uri.EscapeDataString(message));
        }
        catch (FormatException)
        {
            throw new PlatformSmsDeliveryException("SMS provider URL template is invalid.");
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps ||
            uri.UserInfo.Length > 0)
        {
            throw new PlatformSmsDeliveryException("SMS provider must use an absolute HTTPS URL.");
        }

        return uri;
    }

    private static HttpClient CreateHttpClient()
    {
        // Redirects could forward OTPs and provider credentials to another host.
        return new HttpClient(new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.None
        })
        {
            Timeout = RequestTimeout
        };
    }

    [GeneratedRegex(@"^\+?[0-9]{8,20}$", RegexOptions.CultureInvariant)]
    private static partial Regex PhoneNumberPattern();
}

public sealed class PlatformSmsDeliveryException(string message) : Exception(message)
{
}

internal sealed record PlatformSmsSendResult(
    bool Accepted,
    int? HttpStatusCode,
    string? ProviderResponse,
    string? FailureReason,
    string? FailureMessage);
