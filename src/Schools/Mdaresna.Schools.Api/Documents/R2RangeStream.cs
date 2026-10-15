namespace Mdaresna.Schools.Api.Documents;

/// <summary>One 256 KiB ciphertext page per open stream. Object revision is pinned by the fetch delegate.</summary>
public sealed class R2RangeStream(long length, Func<long, int, CancellationToken, Task<byte[]>> fetch, CancellationToken cancellationToken) : Stream
{
    private const int PageSize = 256 * 1024;
    private long _position;
    private long _pageStart = -1;
    private byte[] _page = [];
    private bool _disposed;
    public override bool CanRead => !_disposed;
    public override bool CanSeek => !_disposed;
    public override bool CanWrite => false;
    public override long Length => length;
    public override long Position { get => _position; set => Seek(value, SeekOrigin.Begin); }
    public async Task PrefetchAsync(CancellationToken ct)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_position >= length || (_position >= _pageStart && _position < _pageStart + _page.Length)) return;
        var start = _position / PageSize * PageSize;
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(ct, cancellationToken);
        var expected = (int)Math.Min(PageSize, length - start);
        var page = await fetch(start, expected, linked.Token).ConfigureAwait(false);
        if (page.Length != expected) throw new EndOfStreamException("Truncated R2 range.");
        _page = page; _pageStart = start;
    }
    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken ct = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ct.ThrowIfCancellationRequested(); cancellationToken.ThrowIfCancellationRequested();
        if (buffer.Length == 0 || _position >= length) return 0;
        await PrefetchAsync(ct).ConfigureAwait(false);
        var offset = (int)(_position - _pageStart);
        var count = Math.Min(buffer.Length, _page.Length - offset);
        _page.AsMemory(offset, count).CopyTo(buffer); _position += count; return count;
    }
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken ct) => ReadAsync(buffer.AsMemory(offset, count), ct).AsTask();
    // UploadedFileCipher currently performs synchronous chunk reads. Each miss is bounded by the SDK timeout and request cancellation.
    public override int Read(byte[] buffer, int offset, int count) => ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask().GetAwaiter().GetResult();
    public override long Seek(long offset, SeekOrigin origin)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var position = checked(origin switch { SeekOrigin.Begin => offset, SeekOrigin.Current => _position + offset,
            SeekOrigin.End => length + offset, _ => throw new ArgumentOutOfRangeException(nameof(origin)) });
        if (position < 0) throw new IOException("Negative stream position.");
        return _position = position;
    }
    protected override void Dispose(bool disposing) { _disposed = true; _page = []; base.Dispose(disposing); }
    public override void Flush() { }
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
