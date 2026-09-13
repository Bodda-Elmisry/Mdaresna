using System.Text.RegularExpressions;

namespace Mdaresna.Platform.Domain.Access;

public readonly partial record struct PermissionCode
{
    private PermissionCode(string value) => Value = value;

    public string Value { get; }

    public static PermissionCode Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim().ToLowerInvariant();

        if (normalized.Length > 100 || !ValidPermissionCode().IsMatch(normalized))
        {
            throw new ArgumentException(
                "Permission codes must use dot-separated lowercase words.",
                nameof(value));
        }

        return new PermissionCode(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex(
        @"^[a-z0-9]+(?:[.-][a-z0-9]+)*(?:\.[a-z0-9]+(?:[.-][a-z0-9]+)*)+$",
        RegexOptions.CultureInvariant)]
    private static partial Regex ValidPermissionCode();
}
