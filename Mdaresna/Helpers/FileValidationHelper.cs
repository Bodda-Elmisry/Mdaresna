using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;

namespace Mdaresna.Helpers
{
    public static class FileValidationHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private static readonly string[] AllowedMimeTypes = { "image/jpeg", "image/png", "image/gif", "image/webp" };
        private const long MaxFileSizeInBytes = 5 * 1024 * 1024; // 5 MB

        public static bool ValidateImage(IFormFile file, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (file == null || file.Length == 0)
            {
                errorMessage = "File is empty or not provided.";
                return false;
            }

            // 1. Validate File Size
            if (file.Length > MaxFileSizeInBytes)
            {
                errorMessage = $"File size exceeds the maximum limit of {MaxFileSizeInBytes / (1024 * 1024)} MB.";
                return false;
            }

            // 2. Validate Extension
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
            {
                errorMessage = $"File type {extension} is not allowed. Only image files ({string.Join(", ", AllowedExtensions)}) are permitted.";
                return false;
            }

            // 3. Validate MIME Type
            var mimeType = file.ContentType.ToLowerInvariant();
            if (!AllowedMimeTypes.Contains(mimeType))
            {
                errorMessage = $"MIME type {mimeType} is not allowed.";
                return false;
            }

            // 4. Validate Magic Bytes (Signatures)
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    byte[] buffer = new byte[8];
                    int bytesRead = stream.Read(buffer, 0, 8);

                    if (bytesRead < 4)
                    {
                        errorMessage = "File is too small to contain a valid image signature.";
                        return false;
                    }

                    if (!ValidateMagicBytes(buffer, extension))
                    {
                        errorMessage = "Invalid file signature (magic bytes mismatch). File may be corrupted or disguised.";
                        return false;
                    }
                }
            }
            catch (Exception ex)
            {
                errorMessage = $"Failed to validate file signature: {ex.Message}";
                return false;
            }

            return true;
        }

        private static bool ValidateMagicBytes(byte[] buffer, string extension)
        {
            // JPEG/JPG: FF D8 FF
            if (extension == ".jpg" || extension == ".jpeg")
            {
                return buffer[0] == 0xFF && buffer[1] == 0xD8 && buffer[2] == 0xFF;
            }

            // PNG: 89 50 4E 47
            if (extension == ".png")
            {
                return buffer[0] == 0x89 && buffer[1] == 0x50 && buffer[2] == 0x4E && buffer[3] == 0x47;
            }

            // GIF: 47 49 46 38 ("GIF8")
            if (extension == ".gif")
            {
                return buffer[0] == 0x47 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x38;
            }

            // WEBP: RIFF (52 49 46 46) .... WEBP (57 45 42 50)
            if (extension == ".webp")
            {
                return buffer[0] == 0x52 && buffer[1] == 0x49 && buffer[2] == 0x46 && buffer[3] == 0x46;
            }

            return false;
        }
    }
}
