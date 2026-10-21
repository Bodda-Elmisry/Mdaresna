using System.Globalization;
using System.Text.Json;
using StackExchange.Redis;

namespace Mdaresna.Schools.Infrastructure.Identity;

internal sealed record RoutingEntry(SchoolRoutingTarget? Target, long Version, DateTimeOffset ExpiresAtUtc);
internal sealed record RoutingSlot(string Epoch, RoutingEntry? Entry, long Version = -1);
internal interface ISchoolRoutingStore
{
    Task<RoutingSlot> ReadAsync(string key, CancellationToken ct);
    Task<bool> ApplyAsync(string key, RoutingEntry entry, string epoch, bool fill, CancellationToken ct);
}

internal sealed class RedisSchoolRoutingStore(ConfigurationOptions options) : ISchoolRoutingStore, IAsyncDisposable
{
    private readonly Lazy<Task<ConnectionMultiplexer>> _connection = new(() => ConnectionMultiplexer.ConnectAsync(options));
    // The guard has no TTL. Losing it through eviction/restart changes the epoch and rejects in-flight fills.
    private const string ReadScript = """
        if redis.call('EXISTS',KEYS[1]) == 0 then
          redis.call('DEL',KEYS[2])
          redis.call('HMSET',KEYS[1],'epoch',ARGV[1],'version','-1','blocked','0')
        end
        return {redis.call('HGET',KEYS[1],'epoch'),redis.call('HGET',KEYS[1],'version'),
          redis.call('HGET',KEYS[1],'blocked'),redis.call('GET',KEYS[2]) or ''}
        """;
    private const string WriteScript = """
        if redis.call('HGET',KEYS[1],'epoch') ~= ARGV[1] then return 0 end
        local old = redis.call('HGET',KEYS[1],'version')
        if old ~= '-1' and ARGV[2] < old then return 0 end
        if old == ARGV[2] then
          if redis.call('HGET',KEYS[1],'blocked') == '1' or ARGV[6] ~= '1' or redis.call('EXISTS',KEYS[2]) == 1 then return 0 end
        end
        if ARGV[3] == '1' then redis.call('DEL',KEYS[2])
        else
          redis.call('SET',KEYS[2],ARGV[4])
          redis.call('PEXPIREAT',KEYS[2],ARGV[5])
        end
        redis.call('HMSET',KEYS[1],'version',ARGV[2],'blocked',ARGV[3])
        return 1
        """;

    private async Task<IDatabase> Database(CancellationToken ct) => (await _connection.Value.WaitAsync(ct)).GetDatabase();
    public async Task<RoutingSlot> ReadAsync(string key, CancellationToken ct)
    {
        var db = await Database(ct);
        var result = (RedisResult[])(await db.ScriptEvaluateAsync(ReadScript,
            [key + ":guard", key + ":data"], [Guid.NewGuid().ToString("N")]).WaitAsync(ct))!;
        var version = long.Parse((string)result[1]!, CultureInfo.InvariantCulture);
        RoutingEntry? entry = null;
        if ((string)result[2]! == "1") entry = new(null, version, DateTimeOffset.MaxValue);
        else if (!string.IsNullOrEmpty((string)result[3]!))
        {
            var data = JsonSerializer.Deserialize<RoutingEntry>((string)result[3]!);
            if (data is not null && data.Version == version && data.ExpiresAtUtc > DateTimeOffset.UtcNow) entry = data;
        }
        return new((string)result[0]!, entry, version);
    }
    public async Task<bool> ApplyAsync(string key, RoutingEntry entry, string epoch, bool fill, CancellationToken ct)
    {
        if (entry.Version < 0) throw new ArgumentOutOfRangeException(nameof(entry));
        var db = await Database(ct);
        return (int)(await db.ScriptEvaluateAsync(WriteScript, [key + ":guard", key + ":data"],
            [epoch, entry.Version.ToString("D20", CultureInfo.InvariantCulture), entry.Target is null ? "1" : "0",
                JsonSerializer.Serialize(entry), entry.ExpiresAtUtc.ToUnixTimeMilliseconds(), fill ? "1" : "0"]).WaitAsync(ct)) == 1;
    }
    public async ValueTask DisposeAsync()
    {
        if (_connection.IsValueCreated)
        {
            try { await (await _connection.Value).DisposeAsync(); } catch (RedisException) { }
        }
    }
}
