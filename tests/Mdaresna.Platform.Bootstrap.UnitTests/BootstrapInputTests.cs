using Mdaresna.Platform.Bootstrap;
using Xunit;

namespace Mdaresna.Platform.Bootstrap.UnitTests;

public sealed class BootstrapInputTests
{
    private static readonly string OperationId = Guid.NewGuid().ToString("D");

    [Fact]
    public void UsesTheFixedFirstOwnerPhone()
    {
        var options = BootstrapOptions.Parse(Args("App Manager", OperationId));

        Assert.Equal("00967777661929", options.Phone);
        Assert.Equal("App Manager", options.DisplayName);
        Assert.False(options.DryRun);
    }

    [Fact]
    public void RejectsPhoneOverrides()
    {
        Assert.Throws<BootstrapUsageException>(() =>
            BootstrapOptions.Parse([.. Args("App Manager", OperationId), "--phone", "001234567890"]));
    }

    [Fact]
    public void DryRunDoesNotRequireAttestation()
    {
        var options = BootstrapOptions.Parse(Args("App Manager", OperationId, "--dry-run"));

        Assert.True(options.DryRun);
    }

    [Fact]
    public void RejectsEmptyOperationId()
    {
        Assert.Throws<BootstrapUsageException>(() =>
            BootstrapOptions.Parse(Args("App Manager", Guid.Empty.ToString("D"))));
    }

    [Fact]
    public void RejectsDuplicateDryRun()
    {
        Assert.Throws<BootstrapUsageException>(() =>
            BootstrapOptions.Parse(Args("App Manager", OperationId, "--dry-run", "--dry-run")));
    }

    [Fact]
    public void RejectsOldPhoneAttestationFlag()
    {
        Assert.Throws<BootstrapUsageException>(() =>
            BootstrapOptions.Parse(Args("App Manager", OperationId, "--attest-phone-verified")));
    }

    private static string[] Args(
        string name,
        string operationId,
        params string[] flags) =>
        ["--display-name", name, "--operation-id", operationId, .. flags];
}
