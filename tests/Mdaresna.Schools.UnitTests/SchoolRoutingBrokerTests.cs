using System.Net;
using System.Net.Http.Json;
using System.Text;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Messaging;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using StackExchange.Redis;
using Xunit;

namespace Mdaresna.Schools.UnitTests;

public sealed class SchoolRoutingBrokerTests
{
    [BrokerFact]
    public async Task RealBrokerDeliversUpdatesRetriesFailuresAndReconcilesAfterRedisLosesGuards()
    {
        var id = Guid.NewGuid();
        SchoolRoutingChangedV1 Snapshot(long version, bool active, string host = "first.internal") => new(id, id, "ABC", version,
            active ? new(id, id, "ABC", "Active", "PostgreSql", host, 5432, "fixture", "fixture-secret", true, version) : null);
        var broker = Environment.GetEnvironmentVariable("MDARESNA_ROUTING_BROKER_TEST_URI")!;
        Assert.StartsWith("amqp://fixture:fixture@127.0.0.1:", broker);
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["SchoolRoutingCache:Enabled"] = "true", ["SchoolRoutingCacheConsumer:Enabled"] = "true",
            ["SchoolRoutingCacheConsumer:BrokerUri"] = broker, ["SchoolRoutingCache:Namespace"] = "qa-" + Guid.NewGuid().ToString("N"),
            ["SchoolIdentity:PlatformBaseUrl"] = "https://platform.example.invalid", ["SchoolIdentity:InternalApiKey"] = "fixture"
        }).Build();
        var redisConnection = Environment.GetEnvironmentVariable("MDARESNA_ROUTING_REDIS_TEST_CONNECTION")!;
        await using var store = new RedisSchoolRoutingStore(ConfigurationOptions.Parse(redisConnection));
        var cache = new SchoolRoutingCache(store, config, NullLogger<SchoolRoutingCache>.Instance);
        var handler = new SnapshotHandler { Latest = Snapshot(1, true) };
        var services = new ServiceCollection(); services.AddSingleton(cache);
        services.AddSingleton(new SchoolRoutingSnapshotClient(new HttpClient(handler), config));
        using var provider = services.BuildServiceProvider();
        using var consumer = new SchoolRoutingCacheConsumerService(config, new DevelopmentHost(),
            provider.GetRequiredService<IServiceScopeFactory>(), NullLogger<SchoolRoutingCacheConsumerService>.Instance);
        var factory = new ConnectionFactory { Uri = new Uri(broker) };
        await using var connection = await factory.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));
        await channel.ExchangeDeclareAsync(SchoolRoutingCacheConsumerService.Exchange, ExchangeType.Topic, true);
        await channel.QueueDeclareAsync(SchoolRoutingCacheConsumerService.Queue, true, false, false);
        await channel.QueueBindAsync(SchoolRoutingCacheConsumerService.Queue, SchoolRoutingCacheConsumerService.Exchange, SchoolRoutingChangedV1.MessageType);
        await consumer.StartAsync(default);
        try
        {
            await Publish(Snapshot(1, true));
            await Until(async () => (await store.ReadAsync(cache.Key("ABC"), default)).Version == 1);
            handler.Latest = Snapshot(2, true, "second.internal");
            await Publish(handler.Latest);
            await Until(async () => (await store.ReadAsync(cache.Key("ABC"), default)).Version == 2);
            Assert.Equal("second.internal", (await cache.ResolveAsync("ABC", _ => throw new InvalidOperationException(), default))!.Host);
            handler.Latest = Snapshot(3, false); handler.FailNext = 1;
            await Publish(handler.Latest);
            await Until(async () => (await store.ReadAsync(cache.Key("ABC"), default)).Version == 3);
            Assert.True(handler.Failures > 0);
            Assert.Null(await cache.ResolveAsync("ABC", _ => throw new InvalidOperationException(), default));
            using var raw = await ConnectionMultiplexer.ConnectAsync(redisConnection);
            await raw.GetDatabase().KeyDeleteAsync([cache.Key("ABC") + ":guard", cache.Key("ABC") + ":data"]);
            await Publish(Snapshot(1, true)); // Old event must not resurrect the school after Redis reset.
            await Until(async () => (await store.ReadAsync(cache.Key("ABC"), default)).Version == 3);
            Assert.Null(await cache.ResolveAsync("ABC", _ => throw new InvalidOperationException(), default));
        }
        finally { await consumer.StopAsync(default); }

        async Task Publish(SchoolRoutingChangedV1 data)
        {
            var envelope = IntegrationMessageEnvelope<SchoolRoutingChangedV1>.Create(DateTimeOffset.UtcNow, "mdaresna-platform",
                IntegrationMessageScope.ForSchool(TenantId.From(id), SchoolId.From(id)), data);
            await channel.BasicPublishAsync(SchoolRoutingCacheConsumerService.Exchange, SchoolRoutingChangedV1.MessageType,
                true, new BasicProperties { Persistent = true }, Encoding.UTF8.GetBytes(IntegrationJsonSerializer.Serialize(envelope)));
        }
    }
    private static async Task Until(Func<Task<bool>> check)
    {
        var deadline = DateTime.UtcNow.AddSeconds(15);
        while (DateTime.UtcNow < deadline) { if (await check()) return; await Task.Delay(30); }
        Assert.Fail("Routing event was not applied before the deadline.");
    }
    private sealed class SnapshotHandler : HttpMessageHandler
    {
        public SchoolRoutingChangedV1 Latest = null!;
        public int FailNext;
        public int Failures;
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            if (Interlocked.Exchange(ref FailNext, 0) != 0) { Interlocked.Increment(ref Failures); return Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)); }
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(Latest) });
        }
    }
    public sealed class BrokerFactAttribute : FactAttribute
    {
        public BrokerFactAttribute()
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MDARESNA_ROUTING_BROKER_TEST_URI")) ||
                string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MDARESNA_ROUTING_REDIS_TEST_CONNECTION"))) Skip = "Requires isolated Redis and RabbitMQ test instances.";
        }
    }
    private sealed class DevelopmentHost : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "RoutingQA";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
