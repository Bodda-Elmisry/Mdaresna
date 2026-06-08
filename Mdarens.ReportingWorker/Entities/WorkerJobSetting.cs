namespace Mdarens.ReportingWorker.Entities;

public class WorkerJobSetting
{
    public long Id { get; set; }

    public string JobName { get; set; } = null!;

    public bool IsEnabled { get; set; }

    public int RunsPerDay { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public DateTime? LastRunAt { get; set; }

    public DateTime? NextRunAt { get; set; }

    public int BatchSize { get; set; } = 50;

    public int MaxItemsPerRun { get; set; } = 500;

    public bool ShouldRunNow(DateTime utcNow)
    {
        if (!IsEnabled)
        {
            return false;
        }

        if (NextRunAt.HasValue)
        {
            return utcNow >= NextRunAt.Value;
        }

        var currentTime = utcNow.TimeOfDay;
        var isInsideWindow = StartTime <= EndTime
            ? currentTime >= StartTime && currentTime <= EndTime
            : currentTime >= StartTime || currentTime <= EndTime;

        if (!isInsideWindow)
        {
            return false;
        }

        if (!LastRunAt.HasValue || RunsPerDay <= 0)
        {
            return true;
        }

        var minimumInterval = TimeSpan.FromDays(1d / RunsPerDay);
        return utcNow - LastRunAt.Value >= minimumInterval;
    }
}
