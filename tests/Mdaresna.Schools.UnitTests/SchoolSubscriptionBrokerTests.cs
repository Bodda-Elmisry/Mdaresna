using System.Text;
using Mdaresna.Schools.Application.Identity;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.Schools.Domain.School;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Messaging;
using Mdaresna.Schools.Infrastructure.Persistence;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using RabbitMQ.Client;
using Xunit;

namespace Mdaresna.Schools.UnitTests;

public sealed class SchoolSubscriptionBrokerTests
{
    [SubscriptionBrokerFact]
    public async Task Broker_retries_unavailable_school_and_confirms_applied_snapshot_without_replaying_old_capacity()
    {
        var broker = Environment.GetEnvironmentVariable("MDARESNA_SUBSCRIPTIONS_BROKER_QA")!;
        Assert.StartsWith("amqp://fixture:fixture@127.0.0.1:", broker);
        var factory = new FixtureFactory(); var school = Guid.NewGuid(); var system = Guid.NewGuid(); var subscription = Guid.NewGuid();
        await using (var db = factory.New()) { var now = DateTimeOffset.UtcNow; db.SchoolInformation.Add(SchoolInformation.CreateWithoutUnits(school, school, Guid.NewGuid(), "QA-SCHOOL", "QA", "Private", "SharedSaaS", null, null, Guid.NewGuid(), now, now)); await db.SaveChangesAsync(); }
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["SchoolSubscriptions:ConsumerEnabled"] = "true", ["SchoolSubscriptions:BrokerUri"] = broker }).Build();
        using var services = new ServiceCollection().AddSingleton<ISchoolDbContextFactory>(factory).BuildServiceProvider();
        using var consumer = new SchoolSubscriptionConsumerService(config, new DevelopmentHost(), services.GetRequiredService<IServiceScopeFactory>(), NullLogger<SchoolSubscriptionConsumerService>.Instance);
        var rabbit = new ConnectionFactory { Uri = new Uri(broker) };
        await using var connection = await rabbit.CreateConnectionAsync();
        await using var channel = await connection.CreateChannelAsync(new CreateChannelOptions(true, true));
        await channel.ExchangeDeclareAsync("mdaresna.platform-events", ExchangeType.Topic, true);
        await channel.QueueDeclareAsync("schools.subscriptions.v1", true, false, false);
        await channel.QueueBindAsync("schools.subscriptions.v1", "mdaresna.platform-events", SchoolSubscriptionSnapshotChangedV1.MessageType);
        await channel.QueueDeclareAsync("platform.subscriptions.applied.v1", true, false, false);
        await channel.QueueDeclareAsync("schools.subscriptions.v1.dead", true, false, false);
        await consumer.StartAsync(default);
        try
        {
            var now = DateTimeOffset.UtcNow;
            var data = new SchoolSubscriptionSnapshotChangedV1(school, "QA-SCHOOL", subscription, system, "ACADEMIC", "Student", 2, Guid.NewGuid(), 1, 300, 12, false, now, now.AddYears(1), ["academic.students"]);
            await Publish(data);
            var applied = await ReadReceipt(); Assert.Equal(2, applied.Revision); Assert.Equal(subscription, applied.SubscriptionId);
            Assert.True(factory.Attempts >= 2);
            await Publish(data); Assert.Equal(2, (await ReadReceipt()).Revision);
            await Publish(data with { Revision = 1, BaseCapacity = 100 }); Assert.Equal(2, (await ReadReceipt()).Revision);
            await using (var verify = factory.New()) { var state = await verify.Subscriptions.SingleAsync(); Assert.Equal(300, state.BaseCapacity); Assert.Equal(12, state.AddOnCapacity); Assert.Equal(1, await verify.CapacityMovements.CountAsync(x => x.Action == "SnapshotApplied")); }
            await Publish(data with { Revision = 3 }, "untrusted-producer");
            await Until(async () => await channel.BasicGetAsync("schools.subscriptions.v1.dead", true) != null);
            await using var final = factory.New(); Assert.Equal(2, (await final.Subscriptions.SingleAsync()).Revision);
        }
        finally { await consumer.StopAsync(default); }
        async Task Publish(SchoolSubscriptionSnapshotChangedV1 data, string producer = "mdaresna-platform")
        {
            var envelope = IntegrationMessageEnvelope<SchoolSubscriptionSnapshotChangedV1>.Create(DateTimeOffset.UtcNow, producer, IntegrationMessageScope.ForSchool(TenantId.From(school), SchoolId.From(school)), data);
            await channel.BasicPublishAsync("mdaresna.platform-events", SchoolSubscriptionSnapshotChangedV1.MessageType, true, new BasicProperties { Persistent = true }, Encoding.UTF8.GetBytes(IntegrationJsonSerializer.Serialize(envelope)));
        }
        async Task<SchoolSubscriptionAppliedV1> ReadReceipt()
        {
            BasicGetResult? result = null;
            await Until(async () => (result = await channel.BasicGetAsync("platform.subscriptions.applied.v1", true)) != null);
            return IntegrationJsonSerializer.Deserialize<SchoolSubscriptionAppliedV1>(Encoding.UTF8.GetString(result!.Body.Span)).Data;
        }
    }
    private static async Task Until(Func<Task<bool>> check)
    {
        var until = DateTime.UtcNow.AddSeconds(20);
        while (DateTime.UtcNow < until) { if (await check()) return; await Task.Delay(50); }
        Assert.Fail("Subscription event did not complete before the deadline.");
    }
    private sealed class FixtureFactory : ISchoolDbContextFactory
    {
        private readonly DbContextOptions<SchoolsDbContext> options = new DbContextOptionsBuilder<SchoolsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        public int Attempts;
        public SchoolsDbContext New() => new(options);
        public Task<SchoolLoginTarget?> ResolveAsync(string schoolCode, CancellationToken cancellationToken = default) => Task.FromResult<SchoolLoginTarget?>(null);
        public Task<SchoolsDbContext?> CreateAsync(string schoolCode, CancellationToken cancellationToken = default) => Task.FromResult<SchoolsDbContext?>(Interlocked.Increment(ref Attempts) == 1 ? null : New());
    }
    private sealed class DevelopmentHost : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "SubscriptionQA";
        public string ContentRootPath { get; set; } = ".";
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
    public sealed class SubscriptionBrokerFactAttribute : FactAttribute
    {
        public SubscriptionBrokerFactAttribute() { if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MDARESNA_SUBSCRIPTIONS_BROKER_QA"))) Skip = "Requires an isolated disposable RabbitMQ QA endpoint."; }
    }
}

