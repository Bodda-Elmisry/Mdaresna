using System.Text.RegularExpressions;
using Mdaresna.Platform.Domain.Common;

namespace Mdaresna.Platform.Domain.Billing.Units;

/// <summary>A global Platform catalog item; its code is stable, while its offer may change.</summary>
public sealed partial class UnitType : AggregateRoot
{
    private UnitType(
        Guid id,
        string code,
        string displayName,
        decimal unitPrice,
        string currency,
        bool isActive,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        long version)
    {
        Id = id;
        Code = code;
        DisplayName = displayName;
        UnitPrice = unitPrice;
        Currency = currency;
        IsActive = isActive;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        RestoreVersion(version);
    }

    public Guid Id { get; }
    public string Code { get; }
    public string DisplayName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; }
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static UnitType Create(
        Guid id,
        string code,
        string displayName,
        decimal unitPrice,
        string currency,
        DateTimeOffset occurredAtUtc)
    {
        DomainGuard.NonEmptyGuid(id, nameof(id));
        var now = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        return new UnitType(
            id,
            NormalizeCode(code),
            DomainGuard.RequiredText(displayName, 200, nameof(displayName)),
            ValidatePrice(unitPrice),
            NormalizeCurrency(currency),
            true,
            now,
            now,
            0);
    }

    public bool ChangeOffer(
        string displayName,
        decimal unitPrice,
        string currency,
        DateTimeOffset occurredAtUtc)
    {
        if (!IsActive)
        {
            throw new PlatformDomainException(
                "unit_type.inactive",
                "An inactive unit type cannot be changed.");
        }

        var normalizedName = DomainGuard.RequiredText(displayName, 200, nameof(displayName));
        var validatedPrice = ValidatePrice(unitPrice);
        var normalizedCurrency = NormalizeCurrency(currency);
        if (DisplayName == normalizedName && UnitPrice == validatedPrice &&
            Currency == normalizedCurrency)
        {
            return false;
        }

        DisplayName = normalizedName;
        UnitPrice = validatedPrice;
        Currency = normalizedCurrency;
        UpdatedAtUtc = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        MarkChanged();
        return true;
    }

    public bool Deactivate(DateTimeOffset occurredAtUtc)
    {
        if (!IsActive)
        {
            return false;
        }

        IsActive = false;
        UpdatedAtUtc = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        MarkChanged();
        return true;
    }

    internal static UnitType Rehydrate(
        Guid id,
        string code,
        string displayName,
        decimal unitPrice,
        string currency,
        bool isActive,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        long version)
    {
        DomainGuard.NonEmptyGuid(id, nameof(id));
        var created = DomainGuard.UtcTimestamp(createdAtUtc, nameof(createdAtUtc));
        var updated = DomainGuard.UtcTimestamp(updatedAtUtc, nameof(updatedAtUtc));
        if (updated < created)
        {
            throw new ArgumentException("UpdatedAtUtc cannot precede CreatedAtUtc.");
        }

        return new UnitType(id, NormalizeCode(code),
            DomainGuard.RequiredText(displayName, 200, nameof(displayName)),
            ValidatePrice(unitPrice), NormalizeCurrency(currency), isActive,
            created, updated, version);
    }

    public static string NormalizeCode(string code)
    {
        var normalized = DomainGuard.RequiredText(code, 32, nameof(code)).ToUpperInvariant();
        if (!ValidCode().IsMatch(normalized))
        {
            throw new ArgumentException(
                "Unit type code must contain 3-32 letters, digits, or internal hyphens.",
                nameof(code));
        }

        return normalized;
    }

    public static string NormalizeCurrency(string currency)
    {
        var normalized = DomainGuard.RequiredText(currency, 3, nameof(currency)).ToUpperInvariant();
        if (normalized.Length != 3 || normalized.Any(ch => ch is < 'A' or > 'Z'))
        {
            throw new ArgumentException("Currency must be a three-letter code.", nameof(currency));
        }

        return normalized;
    }

    public static decimal ValidatePrice(decimal price)
    {
        if (price <= 0 || price > 9999999999999999.99m ||
            decimal.Round(price, 2) != price)
        {
            throw new ArgumentOutOfRangeException(nameof(price),
                "Unit price must be positive, fit decimal(18,2), and have at most two decimals.");
        }

        return price;
    }

    [GeneratedRegex("^[A-Z0-9](?:[A-Z0-9-]{1,30}[A-Z0-9])$", RegexOptions.CultureInvariant)]
    private static partial Regex ValidCode();
}
