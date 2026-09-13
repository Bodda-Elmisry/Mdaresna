namespace Mdaresna.Platform.Bootstrap;

internal sealed record BootstrapOptions(
    string DisplayName,
    Guid OperationId,
    bool DryRun)
{
    // This is the one and only initial Platform owner identity in each environment.
    // Changing it requires an explicit, reviewed migration, never a seed rerun.
    public const string FirstOwnerPhone = "00967777661929";
    public string Phone => FirstOwnerPhone;

    public static BootstrapOptions Parse(string[] args)
    {
        if (args.Length == 1 && args[0] is "--help" or "-h")
        {
            throw new BootstrapUsageException(string.Empty);
        }

        var values = new Dictionary<string, string>(StringComparer.Ordinal);
        var dryRun = false;
        for (var index = 0; index < args.Length; index++)
        {
            var argument = args[index];
            if (argument == "--dry-run")
            {
                if (dryRun)
                {
                    throw new BootstrapUsageException("The dry-run flag was repeated.");
                }

                dryRun = true;
                continue;
            }

            if (argument is not ("--display-name" or "--operation-id") ||
                index + 1 >= args.Length ||
                args[index + 1].StartsWith("--", StringComparison.Ordinal) ||
                !values.TryAdd(argument, args[++index]))
            {
                throw new BootstrapUsageException("Invalid or repeated bootstrap argument.");
            }
        }

        if (!values.TryGetValue("--display-name", out var rawName) ||
            !values.TryGetValue("--operation-id", out var rawOperation) ||
            !Guid.TryParse(rawOperation, out var operationId) || operationId == Guid.Empty)
        {
            throw new BootstrapUsageException("Display name and operation ID are required.");
        }

        var displayName = rawName.Trim();
        if (displayName.Length is < 1 or > 200)
        {
            throw new BootstrapUsageException("Display name must be 1-200 characters.");
        }

        return new BootstrapOptions(displayName, operationId, dryRun);
    }
}

internal sealed class BootstrapUsageException(string message) : Exception(message);
