using System.Reflection;
using System.Xml.Linq;
using Mdaresna.Api.Contracts;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Messaging.Abstractions;
using Mdaresna.SharedKernel.Time;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.ArchitectureTests;

public sealed class FoundationDependencyTests
{
    [Fact]
    public void Foundation_projects_follow_the_allowed_dependency_direction()
    {
        AssertLocalReferences(typeof(ApiResponse<>).Assembly);
        AssertLocalReferences(typeof(IClock).Assembly);
        AssertLocalReferences(typeof(TenantId).Assembly);
        AssertLocalReferences(
            typeof(IIntegrationEvent).Assembly,
            "Mdaresna.Tenancy.Abstractions");
        AssertLocalReferences(
            typeof(IOutboxWriter).Assembly,
            "Mdaresna.IntegrationContracts");
    }

    [Fact]
    public void Foundation_projects_do_not_reference_legacy_or_transport_frameworks()
    {
        var forbiddenPrefixes = new[]
        {
            "Mdaresna.Doamin",
            "Mdaresna.Repository",
            "Mdaresna.Infrastructure",
            "Microsoft.AspNetCore",
            "Microsoft.EntityFrameworkCore",
            "RabbitMQ.Client"
        };

        foreach (var assembly in FoundationAssemblies())
        {
            var references = assembly.GetReferencedAssemblies();

            Assert.DoesNotContain(
                references,
                reference =>
                    string.Equals(reference.Name, "Mdaresna", StringComparison.Ordinal) ||
                    forbiddenPrefixes.Any(
                        prefix => string.Equals(reference.Name, prefix, StringComparison.Ordinal) ||
                                  reference.Name?.StartsWith(
                                      $"{prefix}.",
                                      StringComparison.Ordinal) == true));
        }
    }

    [Fact]
    public void Foundation_project_files_follow_the_allowed_reference_graph()
    {
        var repositoryRoot = FindRepositoryRoot();
        var expectedReferences = new Dictionary<string, string[]>
        {
            ["Mdaresna.Api.Contracts"] = [],
            ["Mdaresna.SharedKernel"] = [],
            ["Mdaresna.Tenancy.Abstractions"] = [],
            ["Mdaresna.IntegrationContracts"] = ["Mdaresna.Tenancy.Abstractions"],
            ["Mdaresna.Messaging.Abstractions"] = ["Mdaresna.IntegrationContracts"]
        };
        var buildingBlocksRoot = Path.Combine(repositoryRoot, "src", "BuildingBlocks");
        var actualProjects = Directory
            .EnumerateFiles(buildingBlocksRoot, "*.csproj", SearchOption.AllDirectories)
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(projectName => projectName, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            expectedReferences.Keys.OrderBy(projectName => projectName, StringComparer.Ordinal),
            actualProjects);

        foreach (var (projectName, expectedProjectReferences) in expectedReferences)
        {
            var projectPath = Path.Combine(
                repositoryRoot,
                "src",
                "BuildingBlocks",
                projectName,
                $"{projectName}.csproj");
            var project = XDocument.Load(projectPath);
            var actualProjectReferences = project
                .Descendants("ProjectReference")
                .Select(element => element.Attribute("Include")?.Value)
                .Where(include => !string.IsNullOrWhiteSpace(include))
                .Select(include => Path.GetFileNameWithoutExtension(include!))
                .OrderBy(reference => reference, StringComparer.Ordinal)
                .ToArray();
            var actualPackages = project
                .Descendants("PackageReference")
                .Select(element => element.Attribute("Include")?.Value)
                .Where(include => !string.IsNullOrWhiteSpace(include))
                .ToArray();
            var directFrameworkOrAssemblyReferences = project
                .Descendants()
                .Where(element =>
                    element.Name.LocalName is "FrameworkReference" or "Reference")
                .ToArray();

            Assert.Equal(
                expectedProjectReferences.OrderBy(reference => reference, StringComparer.Ordinal),
                actualProjectReferences);
            Assert.Empty(actualPackages);
            Assert.Empty(directFrameworkOrAssemblyReferences);
        }
    }

    private static IEnumerable<Assembly> FoundationAssemblies()
    {
        yield return typeof(ApiResponse<>).Assembly;
        yield return typeof(IClock).Assembly;
        yield return typeof(TenantId).Assembly;
        yield return typeof(IIntegrationEvent).Assembly;
        yield return typeof(IOutboxWriter).Assembly;
    }

    private static void AssertLocalReferences(
        Assembly assembly,
        params string[] allowedReferences)
    {
        var actualReferences = assembly
            .GetReferencedAssemblies()
            .Select(reference => reference.Name)
            .Where(name => name?.StartsWith("Mdaresna.", StringComparison.Ordinal) == true)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        var expectedReferences = allowedReferences
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expectedReferences, actualReferences);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Mdaresna.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
