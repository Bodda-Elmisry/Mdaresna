using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Mdaresna.Schools.Api.Documents;

public interface IBookPdfRenderer
{
    Task<int> PageCountAsync(Stream pdf, CancellationToken ct);
    Task<byte[]> RenderAsync(Stream pdf, int page, string watermark, CancellationToken ct);
}

/// <summary>Poppler processes receive PDF bytes on stdin, not plaintext temporary files. Renderers must run in a restricted deployment.</summary>
public sealed class BookPdfRenderer(IConfiguration config) : IBookPdfRenderer
{
    private readonly SemaphoreSlim _slots = new(2);
    public async Task<int> PageCountAsync(Stream pdf, CancellationToken ct)
    {
        var bytes = await Run(config["BookDocuments:PdfInfoPath"] ?? "pdfinfo", ["-"], pdf, 65536, ct);
        var text = Encoding.UTF8.GetString(bytes);
        var match = Regex.Match(text, @"^Pages:\s+(\d+)\s*$", RegexOptions.Multiline);
        if (Regex.IsMatch(text, @"^Encrypted:\s+yes", RegexOptions.Multiline) || !match.Success || !int.TryParse(match.Groups[1].Value, out var pages) || pages is < 1 or > 2000)
            throw new InvalidDataException("Use an unpassworded PDF with 1–2000 pages.");
        return pages;
    }
    public async Task<byte[]> RenderAsync(Stream pdf, int page, string watermark, CancellationToken ct)
    {
        var number = page.ToString(CultureInfo.InvariantCulture);
        var bytes = await Run(config["BookDocuments:PdfToPpmPath"] ?? "pdftoppm", ["-f", number, "-l", number, "-singlefile", "-scale-to", "1600", "-",], pdf, 8 * 1024 * 1024, ct);
        return WatermarkedBookPage.FromPpm(bytes, watermark);
    }
    private async Task<byte[]> Run(string executable, string[] args, Stream input, int limit, CancellationToken ct)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct); timeout.CancelAfter(TimeSpan.FromSeconds(30));
        await _slots.WaitAsync(timeout.Token);
        try
        {
            var start = new ProcessStartInfo(executable) { UseShellExecute = false, CreateNoWindow = true, RedirectStandardInput = true, RedirectStandardOutput = true, RedirectStandardError = true };
            start.Environment["LC_ALL"] = "C";
            foreach (var arg in args) start.ArgumentList.Add(arg);
            using var process = new Process { StartInfo = start };
            try
            {
                process.Start();
                using var registration = timeout.Token.Register(() => { try { if (!process.HasExited) process.Kill(true); } catch (InvalidOperationException) { } });
                var stderr = Drain(process.StandardError.BaseStream, 65536, timeout.Token);
                var stdout = Drain(process.StandardOutput.BaseStream, limit, timeout.Token);
                var write = Feed();
                await Task.WhenAll(write, stdout, stderr, process.WaitForExitAsync(timeout.Token));
                if (process.ExitCode != 0) throw new InvalidDataException("PDF rendering failed.");
                return await stdout;
                async Task Feed() { try { await input.CopyToAsync(process.StandardInput.BaseStream, timeout.Token); } finally { process.StandardInput.Close(); } }
            }
            catch { try { if (!process.HasExited) process.Kill(true); } catch (InvalidOperationException) { } throw; }
        }
        finally { _slots.Release(); }
    }
    private static async Task<byte[]> Drain(Stream source, int limit, CancellationToken ct)
    {
        using var target = new MemoryStream(); var buffer = new byte[8192]; int read;
        while ((read = await source.ReadAsync(buffer, ct)) > 0) { if (target.Length + read > limit) throw new InvalidDataException("PDF renderer output exceeded its limit."); await target.WriteAsync(buffer.AsMemory(0, read), ct); }
        return target.ToArray();
    }
}
