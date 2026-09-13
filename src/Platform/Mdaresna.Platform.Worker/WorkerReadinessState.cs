namespace Mdaresna.Platform.Worker;

internal sealed class WorkerReadinessState
{
    private int _isReady;

    public bool IsReady => Volatile.Read(ref _isReady) == 1;

    public void MarkReady() => Interlocked.Exchange(ref _isReady, 1);

    public void MarkNotReady() => Interlocked.Exchange(ref _isReady, 0);
}
