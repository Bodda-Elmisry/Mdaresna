namespace Mdaresna.Schools.Domain.School;

public static class SchoolPresentationRules
{
    public static (string Extension, string ContentType)? ImageType(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 24 && bytes[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            return (".png", "image/png");
        if (bytes.Length >= 12 && bytes[0] == 255 && bytes[1] == 216 && bytes[2] == 255)
            return (".jpg", "image/jpeg");
        if (bytes.Length >= 16 && bytes[..4].SequenceEqual("RIFF"u8) && bytes.Slice(8, 4).SequenceEqual("WEBP"u8))
            return (".webp", "image/webp");
        return null;
    }
}
