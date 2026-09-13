namespace Mdaresna.Platform.Domain.Common;

internal static class DomainGuard
{
    public static DateTimeOffset UtcTimestamp(DateTimeOffset value, string parameterName)
    {
        if (value == default || value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "The timestamp must be non-default and use the UTC offset.",
                parameterName);
        }

        return value;
    }

    public static string RequiredText(
        string? value,
        int maximumLength,
        string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();

        if (normalized.Length > maximumLength)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                $"The value cannot exceed {maximumLength} characters.");
        }

        return normalized;
    }

    public static string? OptionalText(
        string? value,
        int maximumLength,
        string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return RequiredText(value, maximumLength, parameterName);
    }

    public static Guid NonEmptyGuid(Guid value, string parameterName) => value == Guid.Empty
        ? throw new ArgumentException("The identifier cannot be empty.", parameterName)
        : value;
}
