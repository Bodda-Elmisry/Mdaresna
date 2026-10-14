using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Diagnostics.Metrics;
using Mdaresna.Schools.Contracts.Provisioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Mdaresna.Schools.Infrastructure.Identity;

internal sealed record SchoolRoutingTarget(Guid TenantId, Guid SchoolId, string SchoolCode, string Status,
    string Provider, string Host, int Port, string DatabaseName, string CredentialSecretReference, bool RequireTls,
    long RoutingVersion = 0);

internal sealed class SchoolRoutingCache(ISchoolRoutingStore? store, IConfiguration configuration,
    ILogger<SchoolRoutingCache> logger)
{
    private static readonly Meter Metrics = new("Mdaresna.Schools.RoutingCache", "2.0");
    private static readonly Counter<long> Hits = Metrics.CreateCounter<long>("school.routing.cache.hits");
    private static readonly Counter<long> Loads = Metrics.CreateCounter<long>("school.routing.platform.loads");
    private static readonly Counter<long> Failures = Metrics.CreateCounter<long>("school.routing.cache.failures");
    private readonly SemaphoreSlim[] _stripes = Enumerable.Range(0, 64).Select(_ => new SemaphoreSlim(1)).ToArray();
    private readonly SemaphoreSlim _upstream = new(16);
    private long _retryCacheAfterTicks;
    internal string Key(string code) => "mdaresna:school-routing:v2:{" + Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
        (configuration["SchoolRoutingCache:Namespace"] ?? "default") + "\n" +
        configuration["SchoolIdentity:PlatformBaseUrl"]?.TrimEnd('/') + "\n" + code))) + "}";
    private DateTimeOffset Deadline() => DateTimeOffset.UtcNow.AddSeconds(configuration.GetValue<int?>("SchoolRoutingCache:TtlSeconds") ?? 2_592_000);

    public async Task<SchoolRoutingTarget?> ResolveAsync(string code,
        Func<CancellationToken, Task<SchoolRoutingTarget?>> load, CancellationToken ct)
    {
        if (store is null) return await Load();
        var key = Key(code);
        var slot = await Operation(Read, ct);
        if (slot?.Entry is { } hit) { Hits.Add(1); return hit.Target; }
        var stripe = _stripes[(uint)StringComparer.Ordinal.GetHashCode(key) % (uint)_stripes.Length];
        await stripe.WaitAsync(ct);
        try
        {
            for (var attempt = 0; attempt < 2; attempt++)
            {
                slot = await Operation(Read, ct);
                if (slot?.Entry is { } cached) { Hits.Add(1); return cached.Target; }
                var expires = Deadline();
                var target = await Load();
                if (slot is null) return target;
                if (target is not null && (target.Status != "Active" || target.SchoolCode != code)) throw new InvalidOperationException("Invalid routing target.");
                if (target is null) return (await Operation(Read, ct))?.Entry?.Target;
                if (expires <= DateTimeOffset.UtcNow) return target;
                if (await Operation(token => store.ApplyAsync(key, new(target, target.RoutingVersion, expires), slot.Epoch, true, token), ct)) return target;
                var winner = await Operation(Read, ct);
                if (winner?.Entry is { } current) return current.Target;
                if (winner is null) return target;
                // The Redis epoch changed: fetch a fresh snapshot instead of persisting the old one.
            }
            throw new InvalidOperationException("Routing changed during lookup; retry the request.");
        }
        finally { stripe.Release(); }
        async Task<SchoolRoutingTarget?> Load()
        {
            await _upstream.WaitAsync(ct);
            try { Loads.Add(1); return await load(ct); } finally { _upstream.Release(); }
        }
        async Task<RoutingSlot> Read(CancellationToken token)
        {
            var result = await store.ReadAsync(key, token);
            if (result.Entry is { Target: { } value } entry && (value.SchoolCode != code || value.Status != "Active" ||
                value.RoutingVersion != entry.Version || value.SchoolId == Guid.Empty || value.TenantId != value.SchoolId ||
                string.IsNullOrWhiteSpace(value.Host) || string.IsNullOrWhiteSpace(value.DatabaseName)))
                throw new JsonException("Invalid cached routing metadata.");
            return result;
        }
    }

    internal async Task ApplyEventAsync(SchoolRoutingChangedV1 snapshot, CancellationToken ct)
    {
        if (store is null) throw new InvalidOperationException("Routing cache is disabled.");
        var key = Key(snapshot.SchoolCode);
        var slot = await store.ReadAsync(key, ct);
        var data = snapshot.Target;
        var target = data is null ? null : new SchoolRoutingTarget(data.TenantId, data.SchoolId, data.SchoolCode,
            data.Status, data.Provider, data.Host, data.Port, data.DatabaseName, data.CredentialSecretReference,
            data.RequireTls, snapshot.RoutingVersion);
        if (slot.Version >= snapshot.RoutingVersion) return;
        if (!await store.ApplyAsync(key, new(target, snapshot.RoutingVersion, Deadline()), slot.Epoch, false, ct))
        {
            var current = await store.ReadAsync(key, ct);
            if (current.Version < snapshot.RoutingVersion) throw new InvalidOperationException("Routing guard changed; retry the event.");
        }
    }

    private async Task<T?> Operation<T>(Func<CancellationToken, Task<T>> operation, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if (DateTime.UtcNow.Ticks < Interlocked.Read(ref _retryCacheAfterTicks)) return default;
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct); timeout.CancelAfter(TimeSpan.FromMilliseconds(500));
        try { return await operation(timeout.Token).WaitAsync(timeout.Token); }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
        catch (Exception ex) when (ex is StackExchange.Redis.RedisException or OperationCanceledException or JsonException)
        {
            Interlocked.Exchange(ref _retryCacheAfterTicks, DateTime.UtcNow.AddSeconds(5).Ticks);
            Failures.Add(1);
            logger.LogWarning("School routing cache unavailable or invalid; using Platform temporarily.");
            return default;
        }
    }
}
