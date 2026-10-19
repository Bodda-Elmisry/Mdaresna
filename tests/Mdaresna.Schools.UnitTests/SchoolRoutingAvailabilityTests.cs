using System.Net;
using System.Text.Json;
using Mdaresna.Schools.Api.Errors;
using Mdaresna.Schools.Application.Identity;
using Mdaresna.Schools.Infrastructure.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Mdaresna.Schools.UnitTests;

public sealed class SchoolRoutingAvailabilityTests
{
    [Theory]
    [InlineData("timeout")]
    [InlineData("network")]
    [InlineData("upstream503")]
    public async Task UpstreamFailureIsUnavailableAndNextRequestCanRecover(string failure)
    {
        var handler = new Handler(failure);
        var resolver = Resolver(handler);
        await Assert.ThrowsAsync<SchoolRoutingUnavailableException>(() => resolver.ResolveAsync("ABC"));
        // A failed lookup must never cache a missing school or poison subsequent attempts.
        Assert.Null(await resolver.ResolveAsync("ABC"));
        Assert.Equal(2, handler.Calls);
    }

    [Fact]
    public async Task CallerCancellationRemainsCancellation()
    {
        using var cts = new CancellationTokenSource();
        var handler = new Handler("timeout", cts.Cancel);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Resolver(handler).ResolveAsync("ABC", cts.Token));
    }

    [Fact]
    public async Task HandlerReturnsRetryable503WithoutExceptionDetails()
    {
        var context = new DefaultHttpContext { TraceIdentifier = "fixture-correlation" };
        context.Response.Body = new MemoryStream();
        var error = new SchoolRoutingUnavailableException(new HttpRequestException("sensitive upstream details"));
        Assert.True(await new SchoolRoutingExceptionHandler(NullLogger<SchoolRoutingExceptionHandler>.Instance)
            .TryHandleAsync(context, error, default));
        Assert.Equal(503, context.Response.StatusCode);
        Assert.Equal("5", context.Response.Headers.RetryAfter);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        using var json = JsonDocument.Parse(body);
        Assert.False(json.RootElement.GetProperty("isSuccess").GetBoolean());
        Assert.Equal("auth.school_routing_unavailable", json.RootElement.GetProperty("code").GetString());
        Assert.Equal("fixture-correlation", json.RootElement.GetProperty("correlationId").GetString());
        Assert.DoesNotContain("sensitive", body);
    }

    [Fact]
    public async Task HandlerDoesNotMisclassifyOtherExceptionsOrAbortedRequests()
    {
        var handler = new SchoolRoutingExceptionHandler(NullLogger<SchoolRoutingExceptionHandler>.Instance);
        var context = new DefaultHttpContext();
        Assert.False(await handler.TryHandleAsync(context, new InvalidOperationException(), default));
        context.RequestAborted = new CancellationToken(true);
        Assert.False(await handler.TryHandleAsync(context,
            new SchoolRoutingUnavailableException(new TaskCanceledException()), default));
    }

    private static PlatformSchoolLoginTenantResolver Resolver(HttpMessageHandler handler)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["SchoolIdentity:PlatformBaseUrl"] = "https://platform.example.invalid",
            ["SchoolIdentity:InternalApiKey"] = "fixture-key"
        }).Build();
        return new(new HttpClient(handler), config, new SchoolRoutingCache(null, config,
            NullLogger<SchoolRoutingCache>.Instance));
    }

    private sealed class Handler(string failure, Action? cancel = null) : HttpMessageHandler
    {
        public int Calls { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            if (++Calls > 1) return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            cancel?.Invoke();
            return failure switch
            {
                "timeout" => Task.FromException<HttpResponseMessage>(new TaskCanceledException()),
                "network" => Task.FromException<HttpResponseMessage>(new HttpRequestException("fixture")),
                _ => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))
            };
        }
    }
}
