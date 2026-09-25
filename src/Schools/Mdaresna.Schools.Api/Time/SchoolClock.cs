namespace Mdaresna.Schools.Api.Time;

public sealed record SchoolLocalNow(DateTimeOffset UtcNow, DateTime LocalDateTime, DateOnly Date, TimeOnly Time,
    DayOfWeek DayOfWeek, string TimeZoneId);

public sealed class SchoolClock(TimeProvider timeProvider)
{
    public SchoolLocalNow Now(string timeZoneId)
    {
        var zone = Resolve(timeZoneId);
        var utcNow = timeProvider.GetUtcNow();
        var local = TimeZoneInfo.ConvertTime(utcNow, zone).DateTime;
        return new SchoolLocalNow(utcNow, local, DateOnly.FromDateTime(local), TimeOnly.FromDateTime(local),
            local.DayOfWeek, zone.Id);
    }

    public static TimeZoneInfo Resolve(string timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId)) throw new TimeZoneNotFoundException();
        var normalized = timeZoneId.Trim();
        if (normalized != "UTC" && !normalized.Contains('/')) throw new TimeZoneNotFoundException();
        return TimeZoneInfo.FindSystemTimeZoneById(normalized);
    }

    public static IReadOnlyList<SchoolTimeZoneOption> GetTimeZones() => TimeZoneInfo.GetSystemTimeZones()
        .Select(x => new { Zone = x, IanaId = ToIanaId(x.Id) })
        .Where(x => x.IanaId is not null)
        .GroupBy(x => x.IanaId!, StringComparer.Ordinal)
        .Select(x => x.First())
        .OrderBy(x => x.Zone.BaseUtcOffset).ThenBy(x => x.IanaId)
        .Select(x => new SchoolTimeZoneOption(x.IanaId!, x.Zone.DisplayName, x.Zone.BaseUtcOffset.ToString(@"hh\:mm")))
        .ToArray();

    private static string? ToIanaId(string id)
    {
        if (id == "UTC" || id.Contains('/')) return id;
        return TimeZoneInfo.TryConvertWindowsIdToIanaId(id, out var ianaId) ? ianaId : null;
    }
}

public sealed record SchoolTimeZoneOption(string Id, string DisplayName, string BaseUtcOffset);
