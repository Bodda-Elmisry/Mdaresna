using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Mdaresna.Repository.IBServices.Common;
using Microsoft.AspNetCore.Builder.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mdaresna.Infrastructure.BServices.Common
{
    public class FcmService : INotificationService
    {
        private const string DefaultAndroidChannelId = "mdaresna_default";
        private const string FirebaseServiceAccountFileName = "firebase-service-account.json";
        private static readonly TimeSpan StoredNotificationTimeToLive = TimeSpan.FromDays(28);

        private readonly IServiceProvider _serviceProvider;

        public FcmService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;

            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile(ResolveFirebaseServiceAccountPath())
                });
            }
        }

        private static string ResolveFirebaseServiceAccountPath()
        {
            var candidatePaths = new[]
            {
                Path.Combine(AppContext.BaseDirectory, FirebaseServiceAccountFileName),
                Path.Combine(Directory.GetCurrentDirectory(), FirebaseServiceAccountFileName)
            };

            var credentialPath = candidatePaths.FirstOrDefault(File.Exists);
            if (!string.IsNullOrWhiteSpace(credentialPath))
            {
                return credentialPath;
            }

            throw new FileNotFoundException(
                $"Firebase service account file '{FirebaseServiceAccountFileName}' was not found. Checked: {string.Join(", ", candidatePaths)}",
                FirebaseServiceAccountFileName);
        }

        public async Task SendAsync(string token, string title, string body)
        {
            var payload = CreateNotificationPayload(title, body);
            var message = new Message
            {
                Token = token,
                Data = CreateDataPayload(title, payload),
                Notification = new Notification
                {
                    Title = title,
                    Body = payload.DisplayBody
                },
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    TimeToLive = StoredNotificationTimeToLive,
                    Notification = new AndroidNotification
                    {
                        ChannelId = DefaultAndroidChannelId,
                        Sound = "default",
                    }
                },
                Apns = CreateApnsConfig(),
                Webpush = CreateWebpushConfig()
            };

            try
            {
                await FirebaseMessaging.DefaultInstance.SendAsync(message);
            }
            catch (FirebaseMessagingException exception)
            {
                Console.WriteLine($"FCM send failed for token {token}: {exception.Message}");
                if (IsInvalidTokenError(exception))
                {
                    await DeleteInvalidTokensAsync([token]);
                }
                throw;
            }
        }

        public async Task SendToMultiUsersAsync(List<string> tokens, string title, string body)
        {
            var payload = CreateNotificationPayload(title, body);
            var message = new MulticastMessage
            {
                Tokens = tokens,
                Data = CreateDataPayload(title, payload),
                Notification = new Notification
                {
                    Title = title,
                    Body = payload.DisplayBody
                },
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    TimeToLive = StoredNotificationTimeToLive,
                    Notification = new AndroidNotification
                    {
                        ChannelId = DefaultAndroidChannelId,
                        Sound = "default",
                    }
                },
                Apns = CreateApnsConfig(),
                Webpush = CreateWebpushConfig()
            };

            var result = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
            Console.WriteLine("SuccessCount = " + result.SuccessCount.ToString());
            Console.WriteLine("FailureCount = " + result.FailureCount.ToString());

            var invalidTokens = new HashSet<string>(StringComparer.Ordinal);

            for (var i = 0; i < result.Responses.Count; i++)
            {
                if (!result.Responses[i].IsSuccess)
                {
                    var token = tokens[i];
                    var exception = result.Responses[i].Exception;
                    Console.WriteLine($"FCM send failed for token {token}: {exception?.Message}");

                    if (IsInvalidTokenError(exception))
                    {
                        invalidTokens.Add(token);
                    }
                }
            }

            await DeleteInvalidTokensAsync(invalidTokens);
        }

        private static bool IsInvalidTokenError(FirebaseMessagingException? exception)
        {
            return exception?.MessagingErrorCode is MessagingErrorCode.Unregistered
                or MessagingErrorCode.InvalidArgument;
        }

        private async Task DeleteInvalidTokensAsync(IEnumerable<string> tokens)
        {
            var invalidTokens = tokens
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (invalidTokens.Count == 0)
            {
                return;
            }

            try
            {
                await using var scope = _serviceProvider.CreateAsyncScope();
                var dbContext = scope.ServiceProvider
                    .GetRequiredService<Mdaresna.Infrastructure.Data.AppDbContext>();

                var deletedCount = await dbContext.UserDevices
                    .Where(device => invalidTokens.Contains(device.FcmToken))
                    .ExecuteDeleteAsync();

                Console.WriteLine($"Deleted {deletedCount} invalid FCM token(s) from database.");
            }
            catch (Exception dbEx)
            {
                Console.WriteLine($"Failed to delete {invalidTokens.Count} invalid FCM token(s) from DB: {dbEx.Message}");
            }
        }

        private static Dictionary<string, string> CreateDataPayload(
            string title,
            NotificationPayload payload)
        {
            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["body"] = payload.DisplayBody,
                ["rawBody"] = payload.RawBody,
                ["schoolId"] = payload.SchoolId,
                ["schoolBalance"] = payload.SchoolBalance,
                ["targetType"] = payload.TargetType,
                ["targetId"] = payload.TargetId,
                ["studentId"] = payload.StudentId,
                ["classRoomId"] = payload.ClassRoomId,
            };
        }

        private static ApnsConfig CreateApnsConfig()
        {
            var expiresAt = DateTimeOffset.UtcNow
                .Add(StoredNotificationTimeToLive)
                .ToUnixTimeSeconds()
                .ToString(CultureInfo.InvariantCulture);

            return new ApnsConfig
            {
                Headers = new Dictionary<string, string>
                {
                    ["apns-priority"] = "10",
                    ["apns-expiration"] = expiresAt
                },
                Aps = new Aps
                {
                    Sound = "default"
                }
            };
        }

        private static WebpushConfig CreateWebpushConfig()
        {
            return new WebpushConfig
            {
                Headers = new Dictionary<string, string>
                {
                    ["TTL"] = ((int)StoredNotificationTimeToLive.TotalSeconds)
                        .ToString(CultureInfo.InvariantCulture)
                }
            };
        }

        private static NotificationPayload CreateNotificationPayload(string title, string body)
        {
            var payload = new NotificationPayload
            {
                DisplayBody = body,
                RawBody = body,
            };

            if (string.IsNullOrWhiteSpace(body) || !body.Contains('|'))
            {
                return payload;
            }

            var bodyParts = body.Split('|').Select(p => p.Trim()).ToArray();
            if (bodyParts.Length < 2)
            {
                return payload;
            }

            payload.DisplayBody = bodyParts[0];
            var hasKeyValuePairs = bodyParts.Skip(1).Any(p => p.Contains('='));

            if (hasKeyValuePairs)
            {
                foreach (var part in bodyParts.Skip(1))
                {
                    var kv = part.Split('=');
                    if (kv.Length == 2)
                    {
                        var key = kv[0].Trim().ToLower();
                        var val = kv[1].Trim();

                        switch (key)
                        {
                            case "type":
                                payload.TargetType = val;
                                break;
                            case "targetid":
                                payload.TargetId = val;
                                break;
                            case "studentid":
                                payload.StudentId = val;
                                break;
                            case "classroomid":
                                payload.ClassRoomId = val;
                                break;
                            case "schoolid":
                                payload.SchoolId = val;
                                break;
                        }
                    }
                }
            }
            else
            {
                // Legacy fallback parser
                var schoolIdCandidate = bodyParts[^1];
                if (Guid.TryParse(schoolIdCandidate, out _))
                {
                    payload.SchoolId = schoolIdCandidate;
                    var hasBalance =
                        title.Contains("Units Changed", StringComparison.OrdinalIgnoreCase) &&
                        bodyParts.Length > 2;

                    if (hasBalance)
                    {
                        payload.SchoolBalance = bodyParts[^2];
                        payload.DisplayBody = string.Join("|", bodyParts.Take(bodyParts.Length - 2)).Trim();
                    }
                    else
                    {
                        payload.DisplayBody = string.Join("|", bodyParts.Take(bodyParts.Length - 1)).Trim();
                    }
                }
            }

            if (string.IsNullOrWhiteSpace(payload.DisplayBody))
            {
                payload.DisplayBody = body;
            }

            return payload;
        }

        private sealed class NotificationPayload
        {
            public string DisplayBody { get; set; } = string.Empty;
            public string RawBody { get; set; } = string.Empty;
            public string SchoolId { get; set; } = string.Empty;
            public string SchoolBalance { get; set; } = string.Empty;
            public string TargetType { get; set; } = string.Empty;
            public string TargetId { get; set; } = string.Empty;
            public string StudentId { get; set; } = string.Empty;
            public string ClassRoomId { get; set; } = string.Empty;
        }
    }
}
