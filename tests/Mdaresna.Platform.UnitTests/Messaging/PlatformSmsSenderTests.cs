using System.Net;
using Mdaresna.Platform.Infrastructure.Messaging;

namespace Mdaresna.Platform.UnitTests.Messaging;

public sealed class PlatformSmsSenderTests
{
    private static readonly PlatformSmsOptions Options = new()
    {
        ApiUrlTemplate = "https://sms.example.test/send?user={0}&password={1}&sender={2}&to={3}&text={4}",
        Username = "user+one",
        Password = "pass&word",
        SenderName = "مدارسنا",
        SuccessResponsePrefix = "ACCEPTED:"
    };

    [Fact]
    public async Task Escapes_sensitive_values_and_accepts_only_provider_success_prefix()
    {
        var handler = new StubHandler(HttpStatusCode.OK, "ACCEPTED: 12345");
        using var client = new HttpClient(handler);
        using var sender = new PlatformSmsSender(Options, client);

        await sender.SendAsync("00967777661929", "رمز التفعيل: 123456 & لا تشاركه");

        Assert.NotNull(handler.RequestUri);
        Assert.Equal("https", handler.RequestUri!.Scheme);
        Assert.Contains("user=user%2Bone", handler.RequestUri.Query);
        Assert.Contains("password=pass%26word", handler.RequestUri.Query);
        Assert.Contains("to=00967777661929", handler.RequestUri.Query);
        Assert.Contains("%26", handler.RequestUri.Query);
        Assert.DoesNotContain("pass&word", handler.RequestUri.ToString());
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, "ERROR: insufficient balance")]
    [InlineData(HttpStatusCode.Redirect, "ACCEPTED: 12345")]
    [InlineData(HttpStatusCode.OK, "")]
    public async Task Rejects_ambiguous_or_failed_provider_response_without_exposing_secrets(
        HttpStatusCode status,
        string body)
    {
        using var client = new HttpClient(new StubHandler(status, body));
        using var sender = new PlatformSmsSender(Options, client);

        var exception = await Assert.ThrowsAsync<PlatformSmsDeliveryException>(() =>
            sender.SendAsync("00967777661929", "Secret code 123456"));

        Assert.DoesNotContain("123456", exception.ToString());
        Assert.DoesNotContain("pass&word", exception.ToString());
        Assert.DoesNotContain("00967777661929", exception.ToString());
    }

    [Fact]
    public async Task Rejected_provider_response_must_not_leak_an_echoed_otp_phone_or_credentials()
    {
        const string echoedCode = "87654321";
        const string phone = "00967777661929";
        var body = $"ERROR: code={echoedCode}; phone={phone}; password={Options.Password}";
        using var client = new HttpClient(new StubHandler(HttpStatusCode.OK, body));
        using var sender = new PlatformSmsSender(Options, client);

        var exception = await Assert.ThrowsAsync<PlatformSmsDeliveryException>(() =>
            sender.SendAsync(phone, $"Activation code: {echoedCode}"));

        Assert.DoesNotContain(echoedCode, exception.ToString());
        Assert.DoesNotContain(phone, exception.ToString());
        Assert.DoesNotContain(Options.Password!, exception.ToString());
        Assert.DoesNotContain(body, exception.ToString());
    }

    [Fact]
    public async Task Transport_error_must_not_leak_a_sensitive_request_uri()
    {
        const string code = "87654321";
        const string phone = "00967777661929";
        using var client = new HttpClient(new ThrowingHandler(
            $"GET https://sms.example.test/send?password={Options.Password}&to={phone}&text={code} failed"));
        using var sender = new PlatformSmsSender(Options, client);

        var exception = await Assert.ThrowsAsync<PlatformSmsDeliveryException>(() =>
            sender.SendAsync(phone, $"Activation code: {code}"));

        Assert.DoesNotContain(code, exception.ToString());
        Assert.DoesNotContain(phone, exception.ToString());
        Assert.DoesNotContain(Options.Password!, exception.ToString());
    }

    [Fact]
    public async Task Rejects_oversized_provider_response_without_exposing_its_contents()
    {
        const string code = "87654321";
        var body = "ACCEPTED:" + new string('x', 1_000_000) + code;
        using var client = new HttpClient(new StubHandler(HttpStatusCode.OK, body));
        using var sender = new PlatformSmsSender(Options, client);

        var exception = await Assert.ThrowsAsync<PlatformSmsDeliveryException>(() =>
            sender.SendAsync("00967777661929", $"Activation code: {code}"));

        Assert.DoesNotContain(code, exception.ToString());
        Assert.DoesNotContain("00967777661929", exception.ToString());
        Assert.DoesNotContain(Options.Password!, exception.ToString());
    }

    [Fact]
    public async Task Audit_result_binds_oversized_provider_response_to_16_kib()
    {
        const string code = "87654321";
        var body = "ACCEPTED:" + new string('x', 16 * 1024) + code;
        using var client = new HttpClient(new StubHandler(HttpStatusCode.OK, body));
        using var sender = new PlatformSmsSender(Options, client);

        var result = await sender.SendForAuditAsync(
            "00967777661929", $"Activation code: {code}");

        Assert.False(result.Accepted);
        Assert.Equal(200, result.HttpStatusCode);
        Assert.Equal("provider_response_too_large", result.FailureReason);
        Assert.Equal(16 * 1024, result.ProviderResponse!.Length);
        Assert.DoesNotContain(code, result.ProviderResponse);
    }

    [Fact]
    public async Task Audit_result_accepts_response_at_16_kib_boundary()
    {
        var body = "ACCEPTED:" + new string('x', 16 * 1024 - "ACCEPTED:".Length);
        using var client = new HttpClient(new StubHandler(HttpStatusCode.OK, body));
        using var sender = new PlatformSmsSender(Options, client);

        var result = await sender.SendForAuditAsync("00967777661929", "Ready");

        Assert.True(result.Accepted);
        Assert.Equal(200, result.HttpStatusCode);
        Assert.Equal(body, result.ProviderResponse);
        Assert.Null(result.FailureReason);
    }

    [Fact]
    public async Task Legacy_response_mode_accepts_nonempty_provider_acknowledgement_and_long_otp_message()
    {
        var handler = new StubHandler(HttpStatusCode.OK, "123456789");
        using var client = new HttpClient(handler);
        using var sender = new PlatformSmsSender(LegacyOptions(), client);
        const string otpMessage =
            "Mdaresna Platform activation code: 123456. Expires in 10 minutes. Do not share it.";

        Assert.True(otpMessage.Length > 20);
        await sender.SendAsync("00967777661929", otpMessage);

        Assert.NotNull(handler.RequestUri);
        Assert.Contains("text=", handler.RequestUri!.Query);
        Assert.Contains("123456", Uri.UnescapeDataString(handler.RequestUri.Query));
    }

    [Theory]
    [InlineData(HttpStatusCode.OK, "")]
    [InlineData(HttpStatusCode.OK, "   ")]
    [InlineData(HttpStatusCode.OK, "SOMETHING WENT AWRY! Status = Timeout")]
    [InlineData(HttpStatusCode.BadRequest, "123456789")]
    public async Task Legacy_response_mode_rejects_empty_transport_or_legacy_failure(
        HttpStatusCode status,
        string body)
    {
        using var client = new HttpClient(new StubHandler(status, body));
        using var sender = new PlatformSmsSender(LegacyOptions(), client);

        var exception = await Assert.ThrowsAsync<PlatformSmsDeliveryException>(() =>
            sender.SendAsync("00967777661929", "OTP 123456"));

        Assert.DoesNotContain("123456", exception.ToString());
        Assert.DoesNotContain("00967777661929", exception.ToString());
    }

    [Fact]
    public async Task Refuses_missing_success_contract_and_plain_http()
    {
        using var client = new HttpClient(new StubHandler(HttpStatusCode.OK, "ACCEPTED:"));
        using var missingContract = new PlatformSmsSender(new PlatformSmsOptions
        {
            ApiUrlTemplate = Options.ApiUrlTemplate,
            Username = Options.Username,
            Password = Options.Password,
            SenderName = Options.SenderName
        }, client);
        await Assert.ThrowsAsync<PlatformSmsDeliveryException>(() =>
            missingContract.SendAsync("00967777661929", "123456"));

        using var insecure = new PlatformSmsSender(new PlatformSmsOptions
        {
            ApiUrlTemplate = "http://sms.example.test/send?user={0}&password={1}&sender={2}&to={3}&text={4}",
            Username = Options.Username,
            Password = Options.Password,
            SenderName = Options.SenderName,
            SuccessResponsePrefix = Options.SuccessResponsePrefix
        }, client);
        await Assert.ThrowsAsync<PlatformSmsDeliveryException>(() =>
            insecure.SendAsync("00967777661929", "123456"));
    }

    private static PlatformSmsOptions LegacyOptions() => new()
    {
        ApiUrlTemplate = Options.ApiUrlTemplate,
        Username = Options.Username,
        Password = Options.Password,
        SenderName = Options.SenderName,
        SuccessResponsePrefix = PlatformSmsOptions.LegacyNonEmptyResponse
    };

    private sealed class StubHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
    {
        public Uri? RequestUri { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body)
            });
        }
    }

    private sealed class ThrowingHandler(string sensitiveMessage) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            throw new HttpRequestException(sensitiveMessage);
    }
}
