namespace Mdaresna.Platform.Api.Hosting;

internal static class PlatformHostConfiguration
{
    private const string AllowedOriginsSection = "PlatformHost:Cors:AllowedOrigins";

    public static string[] GetValidatedCorsOrigins(
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var configuredOrigins = configuration
            .GetSection(AllowedOriginsSection)
            .Get<string[]>() ?? [];

        var origins = configuredOrigins
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => NormalizeOrigin(origin.Trim(), environment))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        return origins;
    }

    private static string NormalizeOrigin(string origin, IHostEnvironment environment)
    {
        if (origin.Contains('*', StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"CORS origin '{origin}' cannot contain a wildcard.");
        }

        if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) ||
            string.IsNullOrWhiteSpace(uri.Host) ||
            (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp) ||
            uri.AbsolutePath != "/" ||
            !string.IsNullOrEmpty(uri.Query) ||
            !string.IsNullOrEmpty(uri.Fragment))
        {
            throw new InvalidOperationException(
                $"CORS origin '{origin}' must be an HTTP(S) origin without a path, query, or fragment.");
        }

        if (!environment.IsDevelopment() && uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new InvalidOperationException(
                $"CORS origin '{origin}' must use HTTPS outside Development.");
        }

        return uri.GetLeftPart(UriPartial.Authority);
    }
}
