using System.Xml.Linq;

namespace Mdaresna.ArchitectureTests;

public sealed class PlatformDependencyTests
{
    [Fact]
    public void Platform_projects_follow_the_allowed_reference_graph()
    {
        var expectedReferences = new Dictionary<string, string[]>
        {
            ["Mdaresna.Platform.Domain"] =
            [
                "Mdaresna.SharedKernel",
                "Mdaresna.Tenancy.Abstractions"
            ],
            ["Mdaresna.Platform.Contracts"] =
            [
                "Mdaresna.IntegrationContracts",
                "Mdaresna.Tenancy.Abstractions"
            ],
            ["Mdaresna.Platform.Application"] =
            [
                "Mdaresna.Messaging.Abstractions",
                "Mdaresna.Platform.Contracts",
                "Mdaresna.Platform.Domain",
                "Mdaresna.Schools.Contracts",
                "Mdaresna.SharedKernel"
            ],
            ["Mdaresna.Platform.Infrastructure"] =
            [
                "Mdaresna.Platform.Application",
                "Mdaresna.Platform.Domain"
            ],
            ["Mdaresna.Platform.Api"] =
            [
                "Mdaresna.Api.Contracts",
                "Mdaresna.Platform.Application",
                "Mdaresna.Platform.Infrastructure"
            ],
            ["Mdaresna.Platform.Worker"] =
            [
                "Mdaresna.Platform.Application",
                "Mdaresna.Platform.Infrastructure",
                "Mdaresna.Schools.Contracts"
            ]
        };

        foreach (var (projectName, allowedReferences) in expectedReferences)
        {
            var project = LoadPlatformProject(projectName);
            var actualReferences = ProjectReferences(project);

            Assert.Equal(
                allowedReferences.OrderBy(reference => reference, StringComparer.Ordinal),
                actualReferences);
        }
    }

    [Fact]
    public void Shared_api_contracts_do_not_depend_on_an_application_or_host()
    {
        var projectPath = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "BuildingBlocks",
            "Mdaresna.Api.Contracts",
            "Mdaresna.Api.Contracts.csproj");

        var project = XDocument.Load(projectPath);
        Assert.Empty(ProjectReferences(project));
        Assert.Empty(PackageReferences(project));
    }

    [Fact]
    public void Platform_core_projects_do_not_reference_legacy_projects_or_RabbitMQ()
    {
        var forbiddenReferences = new HashSet<string>(StringComparer.Ordinal)
        {
            "Mdaresna",
            "Mdaresna.Doamin",
            "Mdaresna.Infrastructure",
            "Mdaresna.Repository",
            "RabbitMQ.Client"
        };

        foreach (var projectPath in Directory.EnumerateFiles(
                     Path.Combine(FindRepositoryRoot(), "src", "Platform"),
                     "*.csproj",
                     SearchOption.AllDirectories))
        {
            var project = XDocument.Load(projectPath);
            var references = ProjectReferences(project)
                .Concat(PackageReferences(project));

            var forbidden = Path.GetFileNameWithoutExtension(projectPath) ==
                "Mdaresna.Platform.Worker"
                ? forbiddenReferences.Where(x => x != "RabbitMQ.Client").ToHashSet(StringComparer.Ordinal)
                : forbiddenReferences;
            Assert.DoesNotContain(references, forbidden.Contains);
        }
    }

    private static XDocument LoadPlatformProject(string projectName)
    {
        var path = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "Platform",
            projectName,
            $"{projectName}.csproj");

        return XDocument.Load(path);
    }

    private static string[] ProjectReferences(XDocument project) => project
        .Descendants("ProjectReference")
        .Select(element => element.Attribute("Include")?.Value)
        .Where(include => !string.IsNullOrWhiteSpace(include))
        .Select(include => Path.GetFileNameWithoutExtension(include!))
        .OrderBy(reference => reference, StringComparer.Ordinal)
        .ToArray();

    private static string[] PackageReferences(XDocument project) => project
        .Descendants("PackageReference")
        .Select(element => element.Attribute("Include")?.Value)
        .Where(include => !string.IsNullOrWhiteSpace(include))
        .Select(include => include!)
        .OrderBy(reference => reference, StringComparer.Ordinal)
        .ToArray();

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
