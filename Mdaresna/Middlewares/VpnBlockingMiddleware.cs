using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mdaresna.Middlewares
{
    public class VpnBlockingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IMemoryCache _cache;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };

        public VpnBlockingMiddleware(RequestDelegate next, IMemoryCache cache, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _next = next;
            _cache = cache;
            _configuration = configuration;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (_configuration.GetValue<bool>("AppSettings:DisableVpnBlocking"))
            {
                await _next(context);
                return;
            }

            string clientIp = GetClientIp(context);

            if (IsPrivateOrLoopbackIp(clientIp))
            {
                await _next(context);
                return;
            }

            bool isBlocked = await IsVpnOrProxyIpAsync(clientIp);

            if (isBlocked)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "text/plain";
                await context.Response.WriteAsync("VPN_BLOCKED");
                return;
            }

            await _next(context);
        }

        private string GetClientIp(HttpContext context)
        {
            string clientIp = context.Connection.RemoteIpAddress?.ToString();

            if (context.Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
            {
                var ips = forwardedFor.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
                if (ips.Length > 0)
                {
                    clientIp = ips[0].Trim();
                }
            }

            return clientIp;
        }

        private bool IsPrivateOrLoopbackIp(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress)) return true;

            if (ipAddress == "::1" || ipAddress == "127.0.0.1") return true;

            if (IPAddress.TryParse(ipAddress, out var ip))
            {
                if (IPAddress.IsLoopback(ip)) return true;

                byte[] bytes = ip.GetAddressBytes();
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    // Class A: 10.0.0.0 - 10.255.255.255
                    if (bytes[0] == 10) return true;

                    // Class B: 172.16.0.0 - 172.31.255.255
                    if (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) return true;

                    // Class C: 192.168.0.0 - 192.168.255.255
                    if (bytes[0] == 192 && bytes[1] == 168) return true;

                    // Link-local: 169.254.0.0 - 169.254.255.255
                    if (bytes[0] == 169 && bytes[1] == 254) return true;
                }
                else if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                {
                    if (ip.IsIPv6LinkLocal) return true;
                    // Unique Local
                    if (bytes[0] == 0xfc || bytes[0] == 0xfd) return true;
                }
            }
            return false;
        }

        private async Task<bool> IsVpnOrProxyIpAsync(string ipAddress)
        {
            string cacheKey = $"vpn_check_{ipAddress}";

            if (_cache.TryGetValue(cacheKey, out bool isBlocked))
            {
                return isBlocked;
            }

            try
            {
                string url = $"http://ip-api.com/json/{ipAddress}?fields=status,message,proxy,hosting";
                var response = await _httpClient.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();
                    using (var doc = JsonDocument.Parse(jsonString))
                    {
                        var root = doc.RootElement;
                        if (root.TryGetProperty("status", out var statusProp) && statusProp.GetString() == "success")
                        {
                            bool proxy = root.TryGetProperty("proxy", out var proxyProp) && proxyProp.GetBoolean();
                            bool hosting = root.TryGetProperty("hosting", out var hostingProp) && hostingProp.GetBoolean();

                            isBlocked = proxy || hosting;

                            // Cache the result for 12 hours
                            _cache.Set(cacheKey, isBlocked, TimeSpan.FromHours(12));
                            return isBlocked;
                        }
                    }
                }
            }
            catch (Exception)
            {
                // In case of any API error, fail-safe (allow request) to prevent blocking users due to external API failures
                return false;
            }

            return false;
        }
    }
}
