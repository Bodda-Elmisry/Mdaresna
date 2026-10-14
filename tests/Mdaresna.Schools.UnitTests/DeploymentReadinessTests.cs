using Mdaresna.Schools.Api.Documents;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Xunit;

namespace Mdaresna.Schools.UnitTests;

public sealed class DeploymentReadinessTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("invalid-key")]
    public async Task MissingOrInvalidKeyIsUnhealthyWithoutWritingStorage(string? key)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["UploadEncryption:ActiveKeyId"] = "primary",
            ["UploadEncryption:Keys:primary"] = key,
            ["SchoolDocuments:StorageRoot"] = Path.Combine(Path.GetTempPath(), "readiness-test-" + Guid.NewGuid().ToString("N"))
        }).Build();
        var type = typeof(UploadEncryptionConfiguration).Assembly.GetType("Mdaresna.Schools.Api.Documents.DeploymentReadinessCheck")!;
        var check = (IHealthCheck)Activator.CreateInstance(type, config, new TestEnvironment())!;
        var result = await check.CheckHealthAsync(new HealthCheckContext());
        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Null(result.Exception);
        Assert.DoesNotContain("invalid-key", result.Description);
        Assert.False(Directory.Exists(config["SchoolDocuments:StorageRoot"]));
    }

    private sealed class TestEnvironment : IWebHostEnvironment
    {
        public string ApplicationName { get; set; } = "ReadinessTests";
        public string EnvironmentName { get; set; } = "Production";
        public string WebRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider WebRootFileProvider { get; set; } = new NullFileProvider();
        public string ContentRootPath { get; set; } = Path.GetTempPath();
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
