using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace Mdaresna.Middlewares
{
    public class AppCheckMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AppCheckMiddleware> _logger;
        private readonly HttpClient _httpClient;

        private static readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private static List<SecurityKey> _cachedKeys = new List<SecurityKey>();
        private static DateTime _keysExpiration = DateTime.MinValue;

        public AppCheckMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<AppCheckMiddleware> logger)
        {
            _next = next;
            _configuration = configuration;
            _logger = logger;
            _httpClient = new HttpClient();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var isRequired = _configuration.GetValue<bool>("FirebaseAppCheck:Required");
            if (!isRequired)
            {
                await _next(context);
                return;
            }

            // Bypass OPTIONS (preflight) requests
            if (context.Request.Method == "OPTIONS")
            {
                await _next(context);
                return;
            }

            var path = context.Request.Path.Value ?? string.Empty;
            if (ShouldBypassAppCheck(path))
            {
                await _next(context);
                return;
            }

            // Check bypass header
            var bypassHeader = context.Request.Headers["X-App-Check-Bypass-Key"].ToString();
            var configuredBypassKey = _configuration["FirebaseAppCheck:BypassKey"];
            if (!string.IsNullOrEmpty(configuredBypassKey) && bypassHeader == configuredBypassKey)
            {
                await _next(context);
                return;
            }

            var appCheckToken = context.Request.Headers["X-Firebase-AppCheck"].ToString();
            if (string.IsNullOrEmpty(appCheckToken))
            {
                _logger.LogWarning("App Check token is missing.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("App Check token is missing.");
                return;
            }

            var isValid = await ValidateTokenAsync(appCheckToken);
            if (!isValid)
            {
                _logger.LogWarning("App Check token is invalid or expired.");
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsync("App Check token is invalid.");
                return;
            }

            await _next(context);
        }

        private static bool ShouldBypassAppCheck(string path)
        {
            return path.StartsWith("/Images", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
                   path.Equals("/index.html", StringComparison.OrdinalIgnoreCase);
        }

        private async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var projectId = _configuration["FirebaseAppCheck:ProjectId"] ?? "mdaresna";
                var issuer = $"https://firebaseappcheck.googleapis.com/{projectId}";
                var audience = $"projects/{projectId}";

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(5),
                    IssuerSigningKeys = await GetPublicKeysAsync(projectId),
                    ValidateIssuerSigningKey = true
                };

                var handler = new JwtSecurityTokenHandler();
                handler.ValidateToken(token, validationParameters, out _);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "App Check token validation failed.");
                return false;
            }
        }

        private async Task<IEnumerable<SecurityKey>> GetPublicKeysAsync(string projectId)
        {
            if (DateTime.UtcNow < _keysExpiration && _cachedKeys.Count > 0)
            {
                return _cachedKeys;
            }

            await _semaphore.WaitAsync();
            try
            {
                if (DateTime.UtcNow < _keysExpiration && _cachedKeys.Count > 0)
                {
                    return _cachedKeys;
                }

                var url = "https://firebaseappcheck.googleapis.com/v1/jwks";
                var response = await _httpClient.GetFromJsonAsync<JwksResponse>(url);

                var keys = new List<SecurityKey>();
                if (response?.Keys != null)
                {
                    foreach (var key in response.Keys)
                    {
                        if (key.N != null && key.E != null)
                        {
                            var rsaParameters = new RSAParameters
                            {
                                Modulus = Base64UrlEncoder.DecodeBytes(key.N),
                                Exponent = Base64UrlEncoder.DecodeBytes(key.E)
                            };

                            keys.Add(new RsaSecurityKey(rsaParameters) { KeyId = key.Kid });
                        }
                    }
                }

                _cachedKeys = keys;
                _keysExpiration = DateTime.UtcNow.AddHours(6);
                return _cachedKeys;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch Firebase App Check public keys.");
                return _cachedKeys;
            }
            finally
            {
                _semaphore.Release();
            }
        }
    }

    public class JwksResponse
    {
        public List<JwkKey>? Keys { get; set; }
    }

    public class JwkKey
    {
        public string? Kid { get; set; }
        public string? Kty { get; set; }
        public string? Alg { get; set; }
        public string? N { get; set; }
        public string? E { get; set; }
    }
}
