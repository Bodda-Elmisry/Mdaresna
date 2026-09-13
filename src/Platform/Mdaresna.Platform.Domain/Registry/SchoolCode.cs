using System.Text.RegularExpressions;

namespace Mdaresna.Platform.Domain.Registry;

public readonly partial record struct SchoolCode
{
    private SchoolCode(string value) => Value = value;

    public string Value { get; }

    public static SchoolCode Create(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim().ToUpperInvariant();

        if (!ValidSchoolCode().IsMatch(normalized))
        {
            throw new ArgumentException(
                "SchoolCode must contain 3-32 letters, digits, or internal hyphens.",
                nameof(value));
        }

        return new SchoolCode(normalized);
    }

    public override string ToString() => Value;

    [GeneratedRegex("^[A-Z0-9](?:[A-Z0-9-]{1,30}[A-Z0-9])$", RegexOptions.CultureInvariant)]
    private static partial Regex ValidSchoolCode();
}
