namespace Mdaresna.Platform.Contracts.Common;

internal static class ContractGuard
{
    public static Guid NonEmpty(Guid value, string parameterName) => value == Guid.Empty
        ? throw new ArgumentException("The identifier cannot be empty.", parameterName)
        : value;

    public static DateTimeOffset Utc(DateTimeOffset value, string parameterName)
    {
        if (value == default || value.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException(
                "The timestamp must be non-default and use the UTC offset.",
                parameterName);
        }

        return value;
    }

    public static string Text(string? value, int maximumLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();

        if (normalized.Length > maximumLength)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }

        return normalized;
    }

    public static string? OptionalText(string? value, int maximumLength, string parameterName) =>
        string.IsNullOrWhiteSpace(value) ? null : Text(value, maximumLength, parameterName);

    public static TEnum Defined<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum => Enum.IsDefined(value)
            ? value
            : throw new ArgumentOutOfRangeException(parameterName, value, "Unknown enum value.");
}
