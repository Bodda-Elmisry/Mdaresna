using System.Buffers.Binary;
using System.IO.Compression;
using System.Text;
namespace Mdaresna.Schools.Api.Documents;

/// <summary>Bounded P6 decoder and PNG encoder. Watermark pixels are baked into the returned bitmap, never a removable HTML overlay.</summary>
public static class WatermarkedBookPage
{
    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ-:/ .";
    private static readonly string[] Glyphs = ["01110100011001110101110011000101110","00100011000010000100001000010001110","01110100010000100010001000100011111","11110000010000101110000010000111110","00010001100101010010111110001000010","11111100001000011110000010000111110","01110100001000011110100011000101110","11111000010001000100010000100001000","01110100011000101110100011000101110","01110100011000101111000010000101110","01110100011000111111100011000110001","11110100011000111110100011000111110","01111100001000010000100001000001111","11110100011000110001100011000111110","11111100001000011110100001000011111","11111100001000011110100001000010000","01111100001000010111100011000101111","10001100011000111111100011000110001","01110001000010000100001000010001110","00001000010000100001100011000101110","10001100101010011000101001001010001","10000100001000010000100001000011111","10001110111010110101100011000110001","10001110011010110011100011000110001","01110100011000110001100011000101110","11110100011000111110100001000010000","01110100011000110001101011001001101","11110100011000111110101001001010001","01111100001000001110000010000111110","11111001000010000100001000010000100","10001100011000110001100011000101110","10001100011000110001100010101000100","10001100011000110101101011010101010","10001100010101000100010101000110001","10001100010101000100001000010000100","11111000010001000100010001000011111","00000000000000011111000000000000000","00000001000010000000001000010000000","00001000100001000100010001000010000","00000000000000000000000000000000000","00000000000000000000000000010000100"];
    public static byte[] FromPpm(byte[] ppm, string watermark)
    {
        var position = 0;
        string Token()
        {
            while (position < ppm.Length) { if (ppm[position] == '#') { while (position < ppm.Length && ppm[position] != '\n') position++; } else if (char.IsWhiteSpace((char)ppm[position])) position++; else break; }
            var start = position; while (position < ppm.Length && !char.IsWhiteSpace((char)ppm[position])) position++;
            if (position - start is < 1 or > 20) throw new InvalidDataException("Invalid page bitmap."); return Encoding.ASCII.GetString(ppm, start, position - start);
        }
        if (Token() != "P6" || !int.TryParse(Token(), out var width) || !int.TryParse(Token(), out var height) || Token() != "255" || width is < 1 or > 1600 || height is < 1 or > 1600)
            throw new InvalidDataException("Invalid page bitmap.");
        if (position >= ppm.Length || !char.IsWhiteSpace((char)ppm[position])) throw new InvalidDataException("Invalid page bitmap.");
        if (ppm[position++] == '\r' && position < ppm.Length && ppm[position] == '\n') position++;
        var length = checked(width * height * 3); if (ppm.Length - position != length) throw new InvalidDataException("Truncated page bitmap.");
        var pixels = ppm.AsSpan(position, length).ToArray();
        var text = new string(watermark.ToUpperInvariant().Where(c => Alphabet.Contains(c)).Take(160).ToArray());
        for (var y = 25; y < height; y += 220)
            for (var character = 0; character < text.Length; character++)
            {
                var x = 15 + character * 12; if (x + 10 >= width) break;
                var glyph = Glyphs[Alphabet.IndexOf(text[character])];
                for (var row = 0; row < 7; row++) for (var col = 0; col < 5; col++) if (glyph[row * 5 + col] == '1')
                    for (var dy = 0; dy < 2; dy++) for (var dx = 0; dx < 2; dx++) { if (y + row * 2 + dy >= height) continue; var p = ((y + row * 2 + dy) * width + x + col * 2 + dx) * 3; for (var channel = 0; channel < 3; channel++) pixels[p + channel] = (byte)(pixels[p + channel] * .65); }
            }
        using var result = new MemoryStream(); result.Write(new byte[] { 137,80,78,71,13,10,26,10 });
        var ihdr = new byte[13]; BinaryPrimitives.WriteInt32BigEndian(ihdr, width); BinaryPrimitives.WriteInt32BigEndian(ihdr.AsSpan(4), height); ihdr[8] = 8; ihdr[9] = 2; Chunk(result, "IHDR", ihdr);
        using var data = new MemoryStream(); using (var zip = new ZLibStream(data, CompressionLevel.Fastest, true)) { for (var row = 0; row < height; row++) { zip.WriteByte(0); zip.Write(pixels, row * width * 3, width * 3); } }
        Chunk(result, "IDAT", data.ToArray()); Chunk(result, "IEND", []); return result.ToArray();
    }
    private static void Chunk(Stream output, string type, byte[] data)
    {
        var size = new byte[4]; BinaryPrimitives.WriteInt32BigEndian(size, data.Length); output.Write(size); var tag = Encoding.ASCII.GetBytes(type); output.Write(tag); output.Write(data);
        uint crc = uint.MaxValue; foreach (var value in tag.Concat(data)) { crc ^= value; for (var bit = 0; bit < 8; bit++) crc = (crc >> 1) ^ ((crc & 1) != 0 ? 0xedb88320u : 0); }
        BinaryPrimitives.WriteUInt32BigEndian(size, ~crc); output.Write(size);
    }
}
