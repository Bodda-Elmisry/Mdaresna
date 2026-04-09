// Auto-generated API smoke runner
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

record Endpoint(string Method, string Path, JsonElement? Body);

record Config(string BaseUrl, Endpoint[] Endpoints);

class Program
{
    static async Task<int> Main(string[] args)
    {
        var configPath = args.Length > 0 ? args[0] : "tests/endpoints.json";
        if (!File.Exists(configPath))
        {
            Console.Error.WriteLine($"Config file not found: {configPath}");
            return 1;
        }

        var json = await File.ReadAllTextAsync(configPath);
        var config = JsonSerializer.Deserialize<Config>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (config == null || string.IsNullOrWhiteSpace(config.BaseUrl) || config.Endpoints == null || config.Endpoints.Length == 0)
        {
            Console.Error.WriteLine("Invalid config: missing baseUrl or endpoints");
            return 1;
        }

        using var client = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        var results = new List<(Endpoint ep, HttpResponseMessage? resp, string? error, TimeSpan elapsed)>();

        foreach (var ep in config.Endpoints)
        {
            var url = config.BaseUrl + (ep.Path.StartsWith('/') ? ep.Path : "/" + ep.Path);
            var request = new HttpRequestMessage(new HttpMethod(ep.Method), url);

            if (ep.Body.HasValue && ep.Method.ToUpperInvariant() != "GET")
            {
                var bodyJson = JsonSerializer.Serialize(ep.Body.Value, new JsonSerializerOptions { WriteIndented = false });
                request.Content = new StringContent(bodyJson, Encoding.UTF8, "application/json");
            }

            var sw = System.Diagnostics.Stopwatch.StartNew();
            try
            {
                var response = await client.SendAsync(request);
                sw.Stop();
                results.Add((ep, response, null, sw.Elapsed));
            }
            catch (Exception ex)
            {
                sw.Stop();
                results.Add((ep, null, ex.Message, sw.Elapsed));
            }
        }

        PrintResults(results);
        return 0;
    }

    static void PrintResults(List<(Endpoint ep, HttpResponseMessage? resp, string? error, TimeSpan elapsed)> results)
    {
        Console.WriteLine("Method  Path                                         Status   Time(ms)");
        Console.WriteLine(new string('-', 80));

        foreach (var r in results)
        {
            var status = r.resp != null ? ((int)r.resp.StatusCode + " " + r.resp.StatusCode) : "ERR";
            if (r.resp != null && (int)r.resp.StatusCode >= 400)
            {
                var body = r.resp.Content.ReadAsStringAsync().Result;
                status += " - " + Trim(body, 120);
            }
            if (r.error != null)
            {
                status = "ERR - " + r.error;
            }

            Console.WriteLine($"{r.ep.Method,-7}{Trim(r.ep.Path,45),-45}{status,-20}{r.elapsed.TotalMilliseconds,9:F1}");
        }
    }

    static string Trim(string text, int max)
    {
        if (string.IsNullOrEmpty(text)) return "";
        text = text.Replace('\n', ' ').Replace("\r", " ");
        return text.Length <= max ? text : text[..max] + "...";
    }
}
