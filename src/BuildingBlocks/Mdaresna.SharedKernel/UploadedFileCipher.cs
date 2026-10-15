using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Mdaresna.SharedKernel;

/// <summary>Versioned AES-256-GCM envelope. Each file has a derived key and independently authenticated 64 KiB chunks.</summary>
public sealed class UploadedFileCipher(string? activeKeyId, IReadOnlyDictionary<string, string> keys)
{
    public const int ChunkSize = 65536;
    private const int HeaderSize = 128;
    private static readonly byte[] Magic = "MDAENC01"u8.ToArray();
    public static bool IsEncrypted(ReadOnlySpan<byte> bytes) => bytes.Length >= 8 && bytes[..8].SequenceEqual(Magic);

    private byte[] Key(string id)
    {
        if (!keys.TryGetValue(id, out var value)) throw new InvalidOperationException("UploadEncryption key is not configured. File access is denied.");
        byte[] key;
        try { key = Convert.FromBase64String(value); } catch (FormatException) { throw new InvalidOperationException("UploadEncryption key must be a Base64-encoded 256-bit key."); }
        if (key.Length != 32) { CryptographicOperations.ZeroMemory(key); throw new InvalidOperationException("UploadEncryption key must be 256 bits."); }
        return key;
    }

    private static byte[] Nonce(ulong index) { var nonce = new byte[12]; BinaryPrimitives.WriteUInt64BigEndian(nonce.AsSpan(4), index); return nonce; }
    private static byte[] Aad(string scope, byte[] header, int count, ulong? index = null, int size = 0)
    {
        var context = Encoding.UTF8.GetBytes(scope);
        var result = new byte[4 + context.Length + count + (index.HasValue ? 12 : 0)];
        BinaryPrimitives.WriteInt32BigEndian(result, context.Length); context.CopyTo(result, 4); header.AsSpan(0, count).CopyTo(result.AsSpan(4 + context.Length));
        if (index.HasValue) { BinaryPrimitives.WriteUInt64BigEndian(result.AsSpan(result.Length - 12), index.Value); BinaryPrimitives.WriteInt32BigEndian(result.AsSpan(result.Length - 4), size); }
        return result;
    }

    private byte[] Derive(string id, byte[] header, string scope)
    {
        var master = Key(id);
        try { return HKDF.DeriveKey(HashAlgorithmName.SHA256, master, 32, header.AsSpan(72, 32).ToArray(), Encoding.UTF8.GetBytes("mdaresna-upload-v1:" + scope)); }
        finally { CryptographicOperations.ZeroMemory(master); }
    }

    public async Task<(long Size, string Sha256)> EncryptAsync(Stream input, Stream output, string scope, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(activeKeyId) || activeKeyId.Length > 64 || !activeKeyId.All(c => char.IsAsciiLetterOrDigit(c) || c is '-' or '_'))
            throw new InvalidOperationException("UploadEncryption:ActiveKeyId is required; unencrypted uploads are disabled.");
        if (!output.CanSeek) throw new ArgumentException("The encrypted output must be seekable.");
        var header = new byte[HeaderSize]; Magic.CopyTo(header, 0); Encoding.ASCII.GetBytes(activeKeyId).CopyTo(header, 8);
        RandomNumberGenerator.Fill(header.AsSpan(72, 32));
        var derived = Derive(activeKeyId, header, scope);
        using var aes = new AesGcm(derived, 16); CryptographicOperations.ZeroMemory(derived);
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        await output.WriteAsync(header, ct);
        var plain = new byte[ChunkSize]; var cipher = new byte[ChunkSize]; var tag = new byte[16]; long total = 0; ulong index = 0;
        try
        {
            while (true)
            {
                var count = await input.ReadAtLeastAsync(plain, ChunkSize, false, ct); if (count == 0) break;
                aes.Encrypt(Nonce(index), plain.AsSpan(0, count), cipher.AsSpan(0, count), tag, Aad(scope, header, 104, index, count));
                await output.WriteAsync(cipher.AsMemory(0, count), ct); await output.WriteAsync(tag, ct);
                hash.AppendData(plain, 0, count); total = checked(total + count); index++;
            }
            BinaryPrimitives.WriteInt64BigEndian(header.AsSpan(104, 8), total);
            aes.Encrypt(Nonce(ulong.MaxValue), [], [], header.AsSpan(112, 16), Aad(scope, header, 112));
            output.Position = 0; await output.WriteAsync(header, ct); await output.FlushAsync(ct);
            return (total, Convert.ToHexString(hash.GetHashAndReset()).ToLowerInvariant());
        }
        finally { CryptographicOperations.ZeroMemory(plain); }
    }

    /// <summary>Owns input on success. Legacy reads are explicitly enabled only for pre-envelope storage records.</summary>
    public Stream OpenRead(Stream input, string scope, bool allowLegacy = false)
    {
        if (!input.CanSeek) throw new ArgumentException("Encrypted storage requires seekable input.");
        var header = new byte[HeaderSize]; var count = input.ReadAtLeast(header, HeaderSize, false); input.Position = 0;
        if (count < 8 || !header.AsSpan(0, 8).SequenceEqual(Magic))
        { if (allowLegacy) return input; throw new CryptographicException("Invalid encrypted file envelope."); }
        if (count != HeaderSize) throw new CryptographicException("Truncated encrypted file envelope.");
        var id = Encoding.ASCII.GetString(header, 8, 64).TrimEnd('\0'); var derived = Derive(id, header, scope);
        var aes = new AesGcm(derived, 16); CryptographicOperations.ZeroMemory(derived);
        try
        {
            aes.Decrypt(Nonce(ulong.MaxValue), [], header.AsSpan(112, 16), [], Aad(scope, header, 112));
            var size = BinaryPrimitives.ReadInt64BigEndian(header.AsSpan(104, 8));
            if (size < 0 || size > long.MaxValue - HeaderSize - (size / ChunkSize + 1) * 16 || input.Length != HeaderSize + size + ((size + ChunkSize - 1) / ChunkSize) * 16)
                throw new CryptographicException("Invalid encrypted file length.");
            return new DecryptedStream(input, aes, header, scope, size);
        }
        catch { aes.Dispose(); throw; }
    }

    public byte[] EncryptBytes(byte[] input, string scope)
    {
        using var source = new MemoryStream(input, false); using var output = new MemoryStream();
        EncryptAsync(source, output, scope, CancellationToken.None).GetAwaiter().GetResult(); return output.ToArray();
    }

    public byte[] DecryptImageBytes(byte[] input, string scope, bool allowLegacy = true)
    {
        // Legacy image signatures are distinct from the envelope: damaged ciphertext never falls back to raw content.
        var legacy = input.Length >= 12 && (input.AsSpan(0, 3).SequenceEqual(new byte[] { 255, 216, 255 }) || input.AsSpan(0, 8).SequenceEqual(new byte[] { 137,80,78,71,13,10,26,10 }) || (input.AsSpan(0, 4).SequenceEqual("RIFF"u8) && input.AsSpan(8, 4).SequenceEqual("WEBP"u8)));
        using var source = new MemoryStream(input, false); using var plain = OpenRead(source, scope, allowLegacy && legacy); using var output = new MemoryStream(); plain.CopyTo(output); return output.ToArray();
    }

    private sealed class DecryptedStream(Stream source, AesGcm aes, byte[] header, string scope, long size) : Stream
    {
        private long _position;
        private long _cached = -1;
        private readonly byte[] _plain = new byte[ChunkSize];
        private readonly byte[] _cipher = new byte[ChunkSize + 16];
        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => size;
        public override long Position { get => _position; set { if (value < 0) throw new ArgumentOutOfRangeException(nameof(value)); _position = value; } }
        public override int Read(byte[] buffer, int offset, int count) => Read(buffer.AsSpan(offset, count));
        public override int Read(Span<byte> buffer)
        {
            var read = 0;
            while (buffer.Length > 0 && _position < size)
            {
                var index = _position / ChunkSize; var count = (int)Math.Min(ChunkSize, size - index * ChunkSize);
                if (_cached != index)
                {
                    _cached = -1;
                    source.Position = HeaderSize + index * (ChunkSize + 16L); source.ReadExactly(_cipher.AsSpan(0, count + 16));
                    aes.Decrypt(Nonce((ulong)index), _cipher.AsSpan(0, count), _cipher.AsSpan(count, 16), _plain.AsSpan(0, count), Aad(scope, header, 104, (ulong)index, count)); _cached = index;
                }
                var offset = (int)(_position % ChunkSize); var take = Math.Min(buffer.Length, count - offset);
                _plain.AsSpan(offset, take).CopyTo(buffer); buffer = buffer[take..]; _position += take; read += take;
            }
            return read;
        }
        public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var read = 0;
            while (buffer.Length > 0 && _position < size)
            {
                var index = _position / ChunkSize; var count = (int)Math.Min(ChunkSize, size - index * ChunkSize);
                if (_cached != index)
                {
                    _cached = -1;
                    source.Position = HeaderSize + index * (ChunkSize + 16L);
                    await source.ReadExactlyAsync(_cipher.AsMemory(0, count + 16), cancellationToken).ConfigureAwait(false);
                    aes.Decrypt(Nonce((ulong)index), _cipher.AsSpan(0, count), _cipher.AsSpan(count, 16), _plain.AsSpan(0, count), Aad(scope, header, 104, (ulong)index, count)); _cached = index;
                }
                var offset = (int)(_position % ChunkSize); var take = Math.Min(buffer.Length, count - offset);
                _plain.AsMemory(offset, take).CopyTo(buffer); buffer = buffer[take..]; _position += take; read += take;
            }
            return read;
        }
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        public override long Seek(long offset, SeekOrigin origin) { Position = checked((origin == SeekOrigin.Begin ? 0 : origin == SeekOrigin.Current ? Position : Length) + offset); return Position; }
        public override void Flush() { }
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        protected override void Dispose(bool disposing) { if (disposing) { CryptographicOperations.ZeroMemory(_plain); aes.Dispose(); source.Dispose(); } base.Dispose(disposing); }
    }
}
