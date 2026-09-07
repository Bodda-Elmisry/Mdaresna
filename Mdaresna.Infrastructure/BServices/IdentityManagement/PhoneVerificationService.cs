using System.Data;
using System.Security.Cryptography;
using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.Identity;
using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.Helpers;
using Mdaresna.Repository.IBServices.IdentityManagement;
using Mdaresna.Repository.IServices.SettingsManagement.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Mdaresna.Infrastructure.BServices.IdentityManagement
{
    public class PhoneVerificationService : IPhoneVerificationService
    {
        private readonly AppDbContext context;
        private readonly ISMSProviderQueryService smsProviderQueryService;
        private readonly AppSettingDTO settings;

        public PhoneVerificationService(
            AppDbContext context,
            ISMSProviderQueryService smsProviderQueryService,
            IOptions<AppSettingDTO> settings)
        {
            this.context = context;
            this.smsProviderQueryService = smsProviderQueryService;
            this.settings = settings.Value;
        }

        public async Task<VerificationDispatchResultDTO> StartAsync(
            User user,
            VerificationPurposeEnum purpose)
        {
            var now = DateTime.UtcNow;
            var phoneNumber = NormalizePhoneNumber(user.PhoneNumber);
            var previousChallenges = await context.VerificationChallenges
                .Where(item => item.PhoneNumber == phoneNumber &&
                               item.UserId == user.Id &&
                               item.Purpose == purpose &&
                               item.Status == VerificationChallengeStatusEnum.Pending &&
                               item.Deleted == false)
                .OrderByDescending(item => item.CreateDate)
                .ToListAsync();

            var activeChallenge = previousChallenges
                .FirstOrDefault(item => item.ExpiresAtUtc > now);
            foreach (var previous in previousChallenges)
            {
                if (previous == activeChallenge)
                    continue;
                previous.Status = VerificationChallengeStatusEnum.Expired;
                previous.LastModifyDate = now;
            }

            if (activeChallenge != null)
            {
                await context.SaveChangesAsync();
                var existingWhatsAppRequest = await context.WhatsAppVerificationRequests
                    .AsNoTracking()
                    .FirstOrDefaultAsync(item =>
                        item.VerificationChallengeId == activeChallenge.Id &&
                        item.Deleted == false &&
                        (item.Status == WhatsAppVerificationRequestStatusEnum.Pending ||
                         item.Status == WhatsAppVerificationRequestStatusEnum.Prepared ||
                         item.Status == WhatsAppVerificationRequestStatusEnum.SentConfirmed));
                if (existingWhatsAppRequest != null)
                {
                    return await BuildDispatchResultAsync(
                        activeChallenge,
                        false,
                        "A WhatsApp verification request is already active");
                }

                return await SendSmsAsync(activeChallenge, user);
            }

            var challenge = new VerificationChallenge
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                PhoneNumber = phoneNumber,
                Purpose = purpose,
                Status = VerificationChallengeStatusEnum.Pending,
                ExpiresAtUtc = now.AddMinutes(
                    Math.Max(1, settings.WhatsAppVerificationRequestExpiryMinutes)),
                CreateDate = now,
                LastModifyDate = now
            };

            context.VerificationChallenges.Add(challenge);
            await context.SaveChangesAsync();
            return await SendSmsAsync(challenge, user);
        }

        public async Task<VerificationDispatchResultDTO> ResendSmsAsync(Guid challengeId)
        {
            var challenge = await GetPendingChallengeAsync(challengeId);
            var user = await context.Users.FirstOrDefaultAsync(item =>
                item.Id == challenge.UserId && item.Deleted == false);
            if (user == null)
                throw new InvalidOperationException("Verification user was not found");

            return await SendSmsAsync(challenge, user);
        }

        public async Task<VerificationConfirmationResultDTO> ConfirmAsync(
            Guid challengeId,
            string code)
        {
            var now = DateTime.UtcNow;
            var challenge = await context.VerificationChallenges.FirstOrDefaultAsync(item =>
                item.Id == challengeId && item.Deleted == false);

            if (challenge == null)
                return FailedConfirmation("Verification challenge was not found");
            if (challenge.Status != VerificationChallengeStatusEnum.Pending)
                return FailedConfirmation("Verification challenge is no longer active");
            if (challenge.ExpiresAtUtc <= now || string.IsNullOrWhiteSpace(challenge.CodeHash))
            {
                challenge.Status = VerificationChallengeStatusEnum.Expired;
                challenge.LastModifyDate = now;
                await context.SaveChangesAsync();
                return FailedConfirmation("Verification code has expired");
            }

            var maxAttempts = Math.Max(1, settings.VerificationCodeMaxAttempts);
            if (challenge.FailedVerificationAttempts >= maxAttempts)
            {
                challenge.Status = VerificationChallengeStatusEnum.Locked;
                challenge.LockedAtUtc = now;
                challenge.LastModifyDate = now;
                await context.SaveChangesAsync();
                return FailedConfirmation("Verification challenge is locked");
            }

            var submittedHash = HashVerificationCode(challenge.Id, code);
            if (!string.Equals(challenge.CodeHash, submittedHash, StringComparison.Ordinal))
            {
                challenge.FailedVerificationAttempts++;
                if (challenge.FailedVerificationAttempts >= maxAttempts)
                {
                    challenge.Status = VerificationChallengeStatusEnum.Locked;
                    challenge.LockedAtUtc = now;
                }

                challenge.LastModifyDate = now;
                await context.SaveChangesAsync();
                return FailedConfirmation("Wrong confirmation key");
            }

            challenge.Status = VerificationChallengeStatusEnum.Verified;
            challenge.VerifiedAtUtc = now;
            challenge.CodeHash = null;
            challenge.LastModifyDate = now;

            var whatsappRequest = await context.WhatsAppVerificationRequests
                .FirstOrDefaultAsync(item =>
                    item.VerificationChallengeId == challenge.Id && item.Deleted == false);
            if (whatsappRequest != null &&
                whatsappRequest.Status != WhatsAppVerificationRequestStatusEnum.Cancelled)
            {
                whatsappRequest.Status = WhatsAppVerificationRequestStatusEnum.Completed;
                whatsappRequest.LastModifyDate = now;
            }

            await context.SaveChangesAsync();
            return new VerificationConfirmationResultDTO
            {
                Confirmed = true,
                UserId = challenge.UserId,
                Purpose = challenge.Purpose
            };
        }

        public async Task<Guid> RequestWhatsAppAsync(Guid challengeId)
        {
            var challenge = await GetPendingChallengeAsync(challengeId);
            var status = await BuildDispatchResultAsync(challenge, false, string.Empty);
            if (!status.CanRequestWhatsApp)
                throw new InvalidOperationException(
                    "WhatsApp can be requested after SMS attempts are exhausted or an SMS attempt fails");

            var existing = await context.WhatsAppVerificationRequests
                .FirstOrDefaultAsync(item =>
                    item.VerificationChallengeId == challenge.Id && item.Deleted == false);
            if (existing != null)
                return existing.Id;

            var now = DateTime.UtcNow;
            var request = new WhatsAppVerificationRequest
            {
                Id = Guid.NewGuid(),
                VerificationChallengeId = challenge.Id,
                Status = WhatsAppVerificationRequestStatusEnum.Pending,
                RequestedAtUtc = now,
                ExpiresAtUtc = now.AddMinutes(
                    Math.Max(1, settings.WhatsAppVerificationRequestExpiryMinutes)),
                CreateDate = now,
                LastModifyDate = now
            };

            context.WhatsAppVerificationRequests.Add(request);
            await context.SaveChangesAsync();
            return request.Id;
        }

        public async Task<WhatsAppPreparedMessageDTO> PrepareWhatsAppAsync(
            Guid requestId,
            Guid adminUserId)
        {
            var now = DateTime.UtcNow;
            var request = await context.WhatsAppVerificationRequests
                .FirstOrDefaultAsync(item => item.Id == requestId && item.Deleted == false);
            if (request == null)
                throw new InvalidOperationException("WhatsApp verification request was not found");
            if (request.ExpiresAtUtc <= now)
            {
                request.Status = WhatsAppVerificationRequestStatusEnum.Expired;
                request.LastModifyDate = now;
                await context.SaveChangesAsync();
                throw new InvalidOperationException("WhatsApp verification request has expired");
            }
            if (request.Status != WhatsAppVerificationRequestStatusEnum.Pending &&
                request.Status != WhatsAppVerificationRequestStatusEnum.Prepared)
                throw new InvalidOperationException("WhatsApp verification request is no longer pending");
            if (request.Status == WhatsAppVerificationRequestStatusEnum.Prepared &&
                request.PreparedByUserId.HasValue && request.PreparedByUserId != adminUserId)
                throw new InvalidOperationException(
                    "WhatsApp verification request is being handled by another administrator");

            var challenge = await context.VerificationChallenges.FirstOrDefaultAsync(item =>
                item.Id == request.VerificationChallengeId && item.Deleted == false);
            if (challenge == null ||
                challenge.Status == VerificationChallengeStatusEnum.Verified ||
                challenge.Status == VerificationChallengeStatusEnum.Locked)
                throw new InvalidOperationException("Verification challenge is no longer active");

            var code = SMSHelper.GenerateConfirmationKey();
            challenge.CodeHash = HashVerificationCode(challenge.Id, code);
            challenge.Status = VerificationChallengeStatusEnum.Pending;
            challenge.FailedVerificationAttempts = 0;
            challenge.ExpiresAtUtc = now.AddMinutes(
                Math.Max(1, settings.VerificationCodeExpiryMinutes));
            challenge.LastModifyDate = now;

            request.Status = WhatsAppVerificationRequestStatusEnum.Prepared;
            request.PreparedAtUtc = now;
            request.PreparedByUserId = adminUserId;
            request.LastModifyDate = now;
            await context.SaveChangesAsync();

            var message = SMSHelper.BuildConfirmationMessage(code, challenge.Purpose);
            var whatsappPhone = NormalizeWhatsAppPhone(challenge.PhoneNumber);
            return new WhatsAppPreparedMessageDTO
            {
                RequestId = request.Id,
                PhoneNumber = challenge.PhoneNumber,
                Message = message,
                WhatsAppUrl = $"https://wa.me/{whatsappPhone}?text={Uri.EscapeDataString(message)}"
            };
        }

        public async Task ConfirmWhatsAppSentAsync(Guid requestId, Guid adminUserId)
        {
            var request = await context.WhatsAppVerificationRequests
                .FirstOrDefaultAsync(item => item.Id == requestId && item.Deleted == false);
            if (request == null)
                throw new InvalidOperationException("WhatsApp verification request was not found");
            if (request.Status == WhatsAppVerificationRequestStatusEnum.SentConfirmed)
                return;
            if (request.Status != WhatsAppVerificationRequestStatusEnum.Prepared)
                throw new InvalidOperationException(
                    "Prepare the WhatsApp message before confirming it as sent");

            var challenge = await context.VerificationChallenges.FirstAsync(item =>
                item.Id == request.VerificationChallengeId);
            var now = DateTime.UtcNow;
            request.Status = WhatsAppVerificationRequestStatusEnum.SentConfirmed;
            request.SentConfirmedAtUtc = now;
            request.SentConfirmedByUserId = adminUserId;
            request.LastModifyDate = now;

            context.VerificationDeliveryAttempts.Add(new VerificationDeliveryAttempt
            {
                Id = Guid.NewGuid(),
                VerificationChallengeId = challenge.Id,
                PhoneNumber = challenge.PhoneNumber,
                Purpose = challenge.Purpose,
                Channel = VerificationDeliveryChannelEnum.WhatsAppManual,
                IsSuccess = true,
                ProviderResponse = "Manually confirmed by application administrator",
                SentByUserId = adminUserId,
                CreateDate = now,
                LastModifyDate = now
            });
            await context.SaveChangesAsync();
        }

        private async Task<VerificationDispatchResultDTO> SendSmsAsync(
            VerificationChallenge challenge,
            User user)
        {
            var now = DateTime.UtcNow;
            if (challenge.Status != VerificationChallengeStatusEnum.Pending ||
                challenge.ExpiresAtUtc <= now)
                throw new InvalidOperationException("Verification challenge is no longer active");

            await using var transaction = await context.Database.BeginTransactionAsync(
                IsolationLevel.Serializable);
            var windowStart = now.AddMinutes(-Math.Max(1, settings.SmsVerificationWindowMinutes));
            var dayStart = now.AddHours(-24);
            var smsAttempts = context.VerificationDeliveryAttempts.Where(item =>
                item.PhoneNumber == challenge.PhoneNumber &&
                item.Purpose == challenge.Purpose &&
                item.Channel == VerificationDeliveryChannelEnum.Sms &&
                item.Deleted == false);

            var windowCount = await smsAttempts.CountAsync(item => item.CreateDate >= windowStart);
            var dayCount = await smsAttempts.CountAsync(item => item.CreateDate >= dayStart);
            var latestAttempt = await smsAttempts
                .OrderByDescending(item => item.CreateDate)
                .FirstOrDefaultAsync();
            var maxWindow = Math.Max(1, settings.SmsVerificationMaxAttemptsPerWindow);
            var maxDay = Math.Max(maxWindow, settings.SmsVerificationMaxAttemptsPerDay);
            var resendAvailableAt = latestAttempt?.CreateDate?.AddSeconds(
                Math.Max(1, settings.SmsVerificationResendCooldownSeconds));

            if (windowCount >= maxWindow || dayCount >= maxDay)
            {
                await transaction.CommitAsync();
                return await BuildDispatchResultAsync(
                    challenge,
                    false,
                    "SMS sending limit reached");
            }
            if (resendAvailableAt.HasValue && resendAvailableAt.Value > now)
            {
                await transaction.CommitAsync();
                return await BuildDispatchResultAsync(
                    challenge,
                    false,
                    "Please wait before requesting another SMS");
            }

            var code = SMSHelper.GenerateConfirmationKey();
            challenge.CodeHash = HashVerificationCode(challenge.Id, code);
            challenge.ExpiresAtUtc = now.AddMinutes(
                Math.Max(1, settings.VerificationCodeExpiryMinutes));
            challenge.FailedVerificationAttempts = 0;
            challenge.LastModifyDate = now;

            var deliveryAttempt = new VerificationDeliveryAttempt
            {
                Id = Guid.NewGuid(),
                VerificationChallengeId = challenge.Id,
                PhoneNumber = challenge.PhoneNumber,
                Purpose = challenge.Purpose,
                Channel = VerificationDeliveryChannelEnum.Sms,
                IsSuccess = false,
                ProviderResponse = "Pending",
                CreateDate = now,
                LastModifyDate = now
            };
            context.VerificationDeliveryAttempts.Add(deliveryAttempt);
            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            var provider = await smsProviderQueryService.GetFirstActive();
            var response = provider == null
                ? "SOMETHING WENT AWRY! No active SMS provider"
                : await SMSHelper.SendConfirmationKey(
                    provider,
                    user,
                    code,
                    challenge.Purpose);
            var success = SMSHelper.IsSuccessfulResponse(response);

            deliveryAttempt.IsSuccess = success;
            deliveryAttempt.ProviderResponse = Truncate(response, 500);
            deliveryAttempt.LastModifyDate = DateTime.UtcNow;
            context.SMSLogs.Add(new SMSLog
            {
                Id = Guid.NewGuid(),
                SMSProviderId = provider?.Id,
                PhoneNumber = challenge.PhoneNumber,
                Message = $"Verification message ({challenge.Purpose})",
                Response = Truncate(response, 2000),
                IsSuccess = success,
                CreateDate = now,
                LastModifyDate = DateTime.UtcNow
            });
            await context.SaveChangesAsync();

            return await BuildDispatchResultAsync(
                challenge,
                success,
                success ? string.Empty : "SMS provider failed; WhatsApp request is available");
        }

        private async Task<VerificationDispatchResultDTO> BuildDispatchResultAsync(
            VerificationChallenge challenge,
            bool smsSent,
            string message)
        {
            var now = DateTime.UtcNow;
            var windowStart = now.AddMinutes(-Math.Max(1, settings.SmsVerificationWindowMinutes));
            var dayStart = now.AddHours(-24);
            var query = context.VerificationDeliveryAttempts.AsNoTracking().Where(item =>
                item.PhoneNumber == challenge.PhoneNumber &&
                item.Purpose == challenge.Purpose &&
                item.Channel == VerificationDeliveryChannelEnum.Sms &&
                item.Deleted == false);

            var windowCount = await query.CountAsync(item => item.CreateDate >= windowStart);
            var dayCount = await query.CountAsync(item => item.CreateDate >= dayStart);
            var latestAttempt = await query
                .OrderByDescending(item => item.CreateDate)
                .FirstOrDefaultAsync();
            var maxWindow = Math.Max(1, settings.SmsVerificationMaxAttemptsPerWindow);
            var maxDay = Math.Max(maxWindow, settings.SmsVerificationMaxAttemptsPerDay);
            var remaining = Math.Max(0, Math.Min(maxWindow - windowCount, maxDay - dayCount));
            var resendAvailableAt = latestAttempt?.CreateDate?.AddSeconds(
                Math.Max(1, settings.SmsVerificationResendCooldownSeconds));
            var failedLastAttempt = latestAttempt != null && !latestAttempt.IsSuccess;
            var canResend = remaining > 0 &&
                            (!resendAvailableAt.HasValue || resendAvailableAt <= now);

            return new VerificationDispatchResultDTO
            {
                ChallengeId = challenge.Id,
                UserId = challenge.UserId,
                Purpose = challenge.Purpose,
                SmsSent = smsSent,
                SmsAttemptsUsed = windowCount,
                SmsAttemptsRemaining = remaining,
                CanResendSms = canResend,
                CanRequestWhatsApp = remaining == 0 || failedLastAttempt,
                ResendAvailableAtUtc = resendAvailableAt,
                CodeExpiresAtUtc = challenge.ExpiresAtUtc,
                Message = message
            };
        }

        private async Task<VerificationChallenge> GetPendingChallengeAsync(Guid challengeId)
        {
            var challenge = await context.VerificationChallenges.FirstOrDefaultAsync(item =>
                item.Id == challengeId && item.Deleted == false);
            if (challenge == null)
                throw new InvalidOperationException("Verification challenge was not found");
            if (challenge.Status != VerificationChallengeStatusEnum.Pending)
                throw new InvalidOperationException("Verification challenge is no longer active");
            if (challenge.ExpiresAtUtc <= DateTime.UtcNow)
            {
                challenge.Status = VerificationChallengeStatusEnum.Expired;
                challenge.LastModifyDate = DateTime.UtcNow;
                await context.SaveChangesAsync();
                throw new InvalidOperationException("Verification challenge has expired");
            }

            return challenge;
        }

        private string NormalizeWhatsAppPhone(string phoneNumber)
        {
            return NormalizePhoneNumber(phoneNumber);
        }

        private string NormalizePhoneNumber(string phoneNumber)
        {
            var trimmed = phoneNumber.Trim();
            var digits = new string(trimmed.Where(char.IsDigit).ToArray());
            if (digits.StartsWith("00", StringComparison.Ordinal))
                return digits[2..];
            if (!trimmed.StartsWith('+') && digits.StartsWith('0'))
                return settings.DefaultPhoneCountryCode.TrimStart('+') + digits[1..];
            return digits;
        }

        private static VerificationConfirmationResultDTO FailedConfirmation(string message)
        {
            return new VerificationConfirmationResultDTO
            {
                Confirmed = false,
                Message = message
            };
        }

        private static string HashVerificationCode(Guid challengeId, string code)
        {
            var hash = Rfc2898DeriveBytes.Pbkdf2(
                code.Trim(),
                challengeId.ToByteArray(),
                100_000,
                HashAlgorithmName.SHA256,
                32);
            return Convert.ToHexString(hash);
        }

        private static string Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return string.Empty;
            return value.Length <= maxLength ? value : value[..maxLength];
        }
    }
}
