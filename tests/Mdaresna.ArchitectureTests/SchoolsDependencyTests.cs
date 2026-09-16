using System.Xml.Linq;

namespace Mdaresna.ArchitectureTests;

public sealed class SchoolsDependencyTests
{
    [Fact]
    public void Schools_projects_follow_the_allowed_reference_graph()
    {
        var expected = new Dictionary<string, string[]>
        {
            ["Mdaresna.Schools.Domain"] = ["Mdaresna.SharedKernel", "Mdaresna.Tenancy.Abstractions"],
            ["Mdaresna.Schools.Contracts"] = ["Mdaresna.IntegrationContracts", "Mdaresna.Tenancy.Abstractions"],
            ["Mdaresna.Schools.Application"] = ["Mdaresna.Messaging.Abstractions", "Mdaresna.Schools.Contracts", "Mdaresna.Schools.Domain", "Mdaresna.SharedKernel"],
            ["Mdaresna.Schools.Infrastructure"] = ["Mdaresna.Schools.Application", "Mdaresna.Schools.Domain"],
            ["Mdaresna.Schools.Api"] = ["Mdaresna.Api.Contracts", "Mdaresna.Schools.Application", "Mdaresna.Schools.Infrastructure"]
        };

        foreach (var (projectName, allowedReferences) in expected)
        {
            var project = XDocument.Load(Path.Combine(FindRepositoryRoot(), "src", "Schools", projectName, $"{projectName}.csproj"));
            var actualReferences = project.Descendants("ProjectReference")
                .Select(element => element.Attribute("Include")?.Value)
                .Where(include => !string.IsNullOrWhiteSpace(include))
                .Select(include => Path.GetFileNameWithoutExtension(include!))
                .OrderBy(reference => reference, StringComparer.Ordinal)
                .ToArray();
            Assert.Equal(allowedReferences.OrderBy(reference => reference, StringComparer.Ordinal), actualReferences);
        }
    }

    [Fact]
    public void Schools_projects_do_not_reference_platform_or_legacy_projects()
    {
        foreach (var projectPath in Directory.EnumerateFiles(
                     Path.Combine(FindRepositoryRoot(), "src", "Schools"), "*.csproj", SearchOption.AllDirectories))
        {
            var references = XDocument.Load(projectPath).Descendants("ProjectReference")
                .Select(element => Path.GetFileNameWithoutExtension(element.Attribute("Include")?.Value));
            Assert.DoesNotContain(references, reference => reference is "Mdaresna" or "Mdaresna.Doamin" or
                "Mdaresna.Infrastructure" or "Mdaresna.Repository" || reference?.StartsWith("Mdaresna.Platform.", StringComparison.Ordinal) == true);
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Mdaresna.sln"))) return directory.FullName;
            directory = directory.Parent;
        }
        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
