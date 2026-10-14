using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json.Nodes;
using Mdaresna.Schools.Infrastructure.DependencyInjection;
using Mdaresna.Schools.Infrastructure.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using Xunit;
using Mdaresna.Schools.Contracts.Provisioning;
using System.Text.Json;

namespace Mdaresna.Schools.UnitTests;

public sealed class SchoolRoutingCacheTests
{
    private static readonly Guid School = Guid.NewGuid();
    private static readonly SchoolRoutingTarget Target = new(School, School, "ABC", "Active",
        "PostgreSql", "database.internal", 5432, "school_fixture", "fixture", true);

    private static IConfiguration Config(params (string Key, string Value)[] extra)
    {
        var values = new Dictionary<string, string?>
        {
            ["SchoolIdentity:PlatformBaseUrl"] = "https://platform.example.invalid",
            ["SchoolIdentity:InternalApiKey"] = "fixture-internal-key",
            ["SchoolIdentity:DatabaseSecrets:fixture"] = "Username=fixture;Password=fixture-first",
            ["SchoolRoutingCache:TtlSeconds"] = "15"
        };
        foreach (var (key, value) in extra) values[key] = value;
        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }

    private static SchoolRoutingCache Cache(FakeCache? store, IConfiguration? config = null) =>
        new(store, config ?? Config(), NullLogger<SchoolRoutingCache>.Instance);

    [Fact]
    public async Task HitsAvoidPlatformAndKeepCredentialsOutOfCache()
    {
        var store = new FakeCache(); var cache = Cache(store); var config = Config();
        var handler = new Handler(); var resolver = new PlatformSchoolLoginTenantResolver(new HttpClient(handler), config, cache);
        var first = await resolver.ResolveAsync(" abc ");
        config["SchoolIdentity:DatabaseSecrets:fixture"] = "Username=fixture;Password=fixture-rotated";
        var second = await resolver.ResolveAsync("ABC");
        Assert.Equal(1, handler.Calls);
        Assert.Contains("fixture-first", first!.ConnectionString);
        Assert.Contains("fixture-rotated", second!.ConnectionString);
        var payload = Encoding.UTF8.GetString(Assert.Single(store.Values).Value);
        Assert.DoesNotContain("Password", payload);
        Assert.DoesNotContain("ConnectionString", payload);
        Assert.DoesNotContain("fixture-first", payload);
    }

    [Fact]
    public async Task ConcurrentColdReadsShareOnePlatformLookup()
    {
        var cache = Cache(new FakeCache()); var calls = 0;
        async Task<SchoolRoutingTarget?> Load(CancellationToken ct) { Interlocked.Increment(ref calls); await Task.Delay(40, ct); return Target; }
        var results = await Task.WhenAll(Enumerable.Range(0, 40).Select(_ => cache.ResolveAsync("ABC", Load, default)));
        Assert.Equal(1, calls); Assert.All(results, x => Assert.Equal(Target, x));
    }

    [Fact]
    public async Task ExpiredMetadataIsNotServedOrExtended()
    {
        var store = new FakeCache(); var cache = Cache(store); var calls = 0;
        Task<SchoolRoutingTarget?> Load(CancellationToken ct) { calls++; return Task.FromResult<SchoolRoutingTarget?>(Target); }
        await cache.ResolveAsync("ABC", Load, default);
        var pair = Assert.Single(store.Values); var json = JsonNode.Parse(pair.Value)!;
        json["ExpiresAtUtc"] = DateTimeOffset.UtcNow.AddMinutes(-1);
        store.Values[pair.Key] = Encoding.UTF8.GetBytes(json.ToJsonString());
        await cache.ResolveAsync("ABC", Load, default);
        Assert.Equal(2, calls); Assert.NotNull(store.LastOptions!.AbsoluteExpiration);
        Assert.Null(store.LastOptions.SlidingExpiration);
    }

    [Fact]
    public async Task RedisFailureFallsBackToPlatform()
    {
        var store = new FakeCache { Fail = true }; var calls = 0;
        var result = await Cache(store).ResolveAsync("ABC", _ => { calls++; return Task.FromResult<SchoolRoutingTarget?>(Target); }, default);
        Assert.Equal(Target, result); Assert.Equal(1, calls);
    }

    [Fact]
    public async Task InvalidJsonFallsBackWithoutServingInvalidEntry()
    {
        var store = new FakeCache(); var cache = Cache(store);
        await cache.ResolveAsync("ABC", _ => Task.FromResult<SchoolRoutingTarget?>(Target), default);
        var key = Assert.Single(store.Values).Key; store.Values[key] = Encoding.UTF8.GetBytes("broken");
        var calls = 0;
        Assert.Equal(Target, await cache.ResolveAsync("ABC", _ => { calls++; return Task.FromResult<SchoolRoutingTarget?>(Target); }, default));
        Assert.Equal(1, calls);
    }

    [Fact]
    public async Task UnknownSchoolsAreNotCachedAndUpstreamErrorsPropagate()
    {
        var store = new FakeCache(); var cache = Cache(store); var calls = 0;
        Task<SchoolRoutingTarget?> Missing(CancellationToken ct) { calls++; return Task.FromResult<SchoolRoutingTarget?>(null); }
        await cache.ResolveAsync("ABC", Missing, default); await cache.ResolveAsync("ABC", Missing, default);
        Assert.Equal(2, calls); Assert.Empty(store.Values);
        await Assert.ThrowsAsync<HttpRequestException>(() => cache.ResolveAsync("ABC", _ => throw new HttpRequestException("fixture"), default));
        Assert.Empty(store.Values);
    }

    [Fact]
    public async Task CancellationDoesNotFallBack()
    {
        using var cts = new CancellationTokenSource(); cts.Cancel();
        var calls = 0;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Cache(new FakeCache()).ResolveAsync("ABC", _ => { calls++; return Task.FromResult<SchoolRoutingTarget?>(Target); }, cts.Token));
        Assert.Equal(0, calls);
    }

    [Fact]
    public async Task AuthoritiesAndSchoolsAreIsolated()
    {
        var store = new FakeCache(); var calls = 0;
        Task<SchoolRoutingTarget?> Load(CancellationToken ct) { calls++; return Task.FromResult<SchoolRoutingTarget?>(Target); }
        await Cache(store).ResolveAsync("ABC", Load, default);
        await Cache(store, Config(("SchoolIdentity:PlatformBaseUrl", "https://other.example.invalid"))).ResolveAsync("ABC", Load, default);
        await Cache(store).ResolveAsync("XYZ", _ => { calls++; return Task.FromResult<SchoolRoutingTarget?>(Target with { SchoolCode = "XYZ" }); }, default);
        Assert.Equal(3, calls); Assert.Equal(3, store.Values.Count);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("2592001")]
    public void InvalidTtlIsRejectedWhenEnabled(string ttl)
    {
        var config = Config(("SchoolRoutingCache:Enabled", "true"), ("SchoolRoutingCache:RedisConnection", "localhost:6379"), ("SchoolRoutingCache:TtlSeconds", ttl));
        Assert.Throws<InvalidOperationException>(() => new ServiceCollection().AddSchoolsInfrastructure(config));
    }

    [Fact]
    public void MonthlyTtlIsAcceptedWhenEnabled()
    {
        var config = Config(("SchoolRoutingCache:Enabled", "true"), ("SchoolRoutingCache:RedisConnection", "localhost:6379"),
            ("SchoolRoutingCache:TtlSeconds", "2592000"));
        new ServiceCollection().AddSchoolsInfrastructure(config);
    }

    [Fact]
    public async Task DefaultTtlIsThirtyDaysAndHitsDoNotExtendIt()
    {
        var config = Config(); config["SchoolRoutingCache:TtlSeconds"] = null;
        var store = new FakeCache(); var cache = Cache(store, config);
        var started = DateTimeOffset.UtcNow;
        await cache.ResolveAsync("ABC", _ => Task.FromResult<SchoolRoutingTarget?>(Target), default);
        var expires = store.LastOptions!.AbsoluteExpiration!.Value;
        Assert.InRange(expires, started.AddDays(30), DateTimeOffset.UtcNow.AddDays(30));
        var payload = Encoding.UTF8.GetString(Assert.Single(store.Values).Value);
        Assert.Equal(expires, JsonNode.Parse(payload)!["ExpiresAtUtc"]!.GetValue<DateTimeOffset>());
        await cache.ResolveAsync("ABC", _ => throw new InvalidOperationException("A cache hit must not request Platform."), default);
        Assert.Equal(expires, store.LastOptions.AbsoluteExpiration);
        Assert.Null(store.LastOptions.SlidingExpiration);
    }

    [Fact]
    public async Task DisabledCachePreservesDirectLookup()
    {
        var handler = new Handler(); var config = Config();
        var resolver = new PlatformSchoolLoginTenantResolver(new HttpClient(handler), config, Cache(null));
        await resolver.ResolveAsync("ABC"); await resolver.ResolveAsync("ABC");
        Assert.Equal(2, handler.Calls);
    }

    [RedisFact]
    public async Task RealRedisSharesMetadataAcrossInstancesAndExpiresIt()
    {
        var connection = Environment.GetEnvironmentVariable("MDARESNA_ROUTING_REDIS_TEST_CONNECTION")!;
        await using var store = new RedisSchoolRoutingStore(ConfigurationOptions.Parse(connection));
        await store.ReadAsync("routing-qa:{" + Guid.NewGuid().ToString("N") + "}", default);
        var config = Config(("SchoolRoutingCache:TtlSeconds", "5"), ("SchoolRoutingCache:Namespace", "qa-" + Guid.NewGuid().ToString("N")));
        var one = new SchoolRoutingCache(store, config, NullLogger<SchoolRoutingCache>.Instance);
        var two = new SchoolRoutingCache(store, config, NullLogger<SchoolRoutingCache>.Instance);
        var calls = 0;
        Task<SchoolRoutingTarget?> Load(CancellationToken ct) { calls++; return Task.FromResult<SchoolRoutingTarget?>(Target); }
        Assert.Equal(Target, await one.ResolveAsync("ABC", Load, default));
        Assert.Equal(Target, await two.ResolveAsync("ABC", Load, default));
        Assert.Equal(1, calls);
        await Task.Delay(5200);
        Assert.Equal(Target, await two.ResolveAsync("ABC", Load, default));
        Assert.Equal(2, calls);
    }

    public sealed class RedisFactAttribute : FactAttribute
    {
        public RedisFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MDARESNA_ROUTING_REDIS_TEST_CONNECTION")))
                Skip = "Set MDARESNA_ROUTING_REDIS_TEST_CONNECTION to an isolated test Redis instance.";
        }
    }

    private static SchoolRoutingChangedV1 Snapshot(SchoolRoutingTarget? target, long version) => new(School, School, "ABC", version,
        target is null ? null : new SchoolRoutingData(School, School, "ABC", "Active", target.Provider, target.Host,
            target.Port, target.DatabaseName, target.CredentialSecretReference, target.RequireTls, version));

    [Fact]
    public async Task EventsUpdateInPlaceIgnoreDuplicatesAndKeepDeletionGuard()
    {
        var store = new FakeCache(); var cache = Cache(store);
        await cache.ResolveAsync("ABC", _ => Task.FromResult<SchoolRoutingTarget?>(Target), default);
        await cache.ApplyEventAsync(Snapshot(Target with { Host = "new.internal" }, 2), default);
        var deadline = store.LastOptions!.AbsoluteExpiration;
        await cache.ApplyEventAsync(Snapshot(Target, 1), default);
        await cache.ApplyEventAsync(Snapshot(Target, 2), default);
        Assert.Equal(deadline, store.LastOptions.AbsoluteExpiration);
        Assert.Equal("new.internal", (await cache.ResolveAsync("ABC", _ => throw new InvalidOperationException(), default))!.Host);
        await cache.ApplyEventAsync(Snapshot(null, 3), default);
        Assert.Empty(store.Values);
        await cache.ApplyEventAsync(Snapshot(Target, 2), default);
        Assert.Null(await cache.ResolveAsync("ABC", _ => throw new InvalidOperationException(), default));
        await cache.ApplyEventAsync(Snapshot(Target, 4), default);
        Assert.NotNull(await cache.ResolveAsync("ABC", _ => throw new InvalidOperationException(), default));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task InFlightFillCannotOverwriteNewerUpdateOrDeletion(bool delete)
    {
        var cache = Cache(new FakeCache());
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var finish = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var pending = cache.ResolveAsync("ABC", async _ => { started.SetResult(); await finish.Task; return Target; }, default);
        await started.Task;
        await cache.ApplyEventAsync(Snapshot(delete ? null : Target with { Host = "new.internal" }, 2), default);
        finish.SetResult();
        var result = await pending;
        if (delete) Assert.Null(result); else Assert.Equal("new.internal", result!.Host);
    }

    [Fact]
    public async Task LostGuardRejectsSnapshotLoadedBeforeRestart()
    {
        var store = new FakeCache(); var cache = Cache(store); var calls = 0;
        var result = await cache.ResolveAsync("ABC", _ =>
        {
            calls++;
            if (calls == 1) { store.Reset(); return Task.FromResult<SchoolRoutingTarget?>(Target); }
            return Task.FromResult<SchoolRoutingTarget?>(Target with { Host = "current.internal", RoutingVersion = 1 });
        }, default);
        Assert.Equal(2, calls); Assert.Equal("current.internal", result!.Host);
    }

    [Fact]
    public async Task EventFailureIsNotSwallowedAsRequestFallback()
    {
        await Assert.ThrowsAsync<RedisConnectionException>(() => Cache(new FakeCache { Fail = true }).ApplyEventAsync(Snapshot(Target, 1), default));
    }

    [RedisFact]
    public async Task RealRedisAtomicallyRejectsOldVersionsAndOldEpochs()
    {
        await using var store = new RedisSchoolRoutingStore(ConfigurationOptions.Parse(Environment.GetEnvironmentVariable("MDARESNA_ROUTING_REDIS_TEST_CONNECTION")!));
        var key = "routing-qa:{" + Guid.NewGuid().ToString("N") + "}";
        var slot = await store.ReadAsync(key, default);
        var deadline = DateTimeOffset.UtcNow.AddSeconds(10);
        Assert.True(await store.ApplyAsync(key, new(Target, 2, deadline), slot.Epoch, false, default));
        Assert.False(await store.ApplyAsync(key, new(Target, 1, deadline), slot.Epoch, true, default));
        Assert.True(await store.ApplyAsync(key, new(null, 3, deadline), slot.Epoch, false, default));
        Assert.Null((await store.ReadAsync(key, default)).Entry!.Target);
        Assert.False(await store.ApplyAsync(key, new(Target, 2, deadline), slot.Epoch, true, default));
        Assert.False(await store.ApplyAsync(key, new(Target, 4, deadline), "obsolete-epoch", true, default));
        Assert.True(await store.ApplyAsync(key, new(Target, 4, deadline), slot.Epoch, false, default));
        Assert.Equal(4, (await store.ReadAsync(key, default)).Version);
    }

    private sealed class Handler : HttpMessageHandler
    {
        public int Calls;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        { Interlocked.Increment(ref Calls); return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(Snapshot(Target, Target.RoutingVersion)) }); }
    }
    private sealed class FakeCache : ISchoolRoutingStore
    {
        public System.Collections.Concurrent.ConcurrentDictionary<string, byte[]> Values = new();
        public bool Fail;
        public DistributedCacheEntryOptions? LastOptions;
        private readonly Dictionary<string, (string Epoch, long Version, bool Blocked)> Guards = new();
        public void Reset() { lock (Guards) { Guards.Clear(); Values.Clear(); } }
        public Task<RoutingSlot> ReadAsync(string key, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (Fail) throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "fixture");
            lock (Guards)
            {
                if (!Guards.TryGetValue(key, out var guard)) Guards[key] = guard = (Guid.NewGuid().ToString("N"), -1, false);
                RoutingEntry? entry = null;
                if (guard.Blocked) entry = new(null, guard.Version, DateTimeOffset.MaxValue);
                else if (Values.TryGetValue(key, out var bytes))
                {
                    var data = JsonSerializer.Deserialize<RoutingEntry>(bytes);
                    if (data?.ExpiresAtUtc > DateTimeOffset.UtcNow) entry = data;
                    else Values.TryRemove(key, out _);
                }
                return Task.FromResult(new RoutingSlot(guard.Epoch, entry, guard.Version));
            }
        }
        public Task<bool> ApplyAsync(string key, RoutingEntry entry, string epoch, bool fill, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            if (Fail) throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "fixture");
            lock (Guards)
            {
                if (!Guards.TryGetValue(key, out var guard)) return Task.FromResult(false);
                if (guard.Epoch != epoch || entry.Version < guard.Version || entry.Version == guard.Version &&
                    (guard.Blocked || !fill || Values.ContainsKey(key))) return Task.FromResult(false);
                Guards[key] = (epoch, entry.Version, entry.Target is null);
                if (entry.Target is null) Values.TryRemove(key, out _);
                else Set(key, JsonSerializer.SerializeToUtf8Bytes(entry), new DistributedCacheEntryOptions { AbsoluteExpiration = entry.ExpiresAtUtc });
                return Task.FromResult(true);
            }
        }
        public byte[]? Get(string key) => Values.TryGetValue(key, out var value) ? value : null;
        public Task<byte[]?> GetAsync(string key, CancellationToken token = default)
        { token.ThrowIfCancellationRequested(); if (Fail) throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "fixture"); return Task.FromResult(Get(key)); }
        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) { Values[key] = value; LastOptions = options; }
        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default) { Set(key, value, options); return Task.CompletedTask; }
        public void Refresh(string key) { }
        public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;
        public void Remove(string key) => Values.TryRemove(key, out _);
        public Task RemoveAsync(string key, CancellationToken token = default) { Remove(key); return Task.CompletedTask; }
    }
}
