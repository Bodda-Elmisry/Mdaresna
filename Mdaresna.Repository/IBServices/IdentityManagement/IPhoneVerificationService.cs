using Mdaresna.Doamin.DTOs.Identity;
using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.UserManagement;

namespace Mdaresna.Repository.IBServices.IdentityManagement
{
    public interface IPhoneVerificationService
    {
        Task<VerificationDispatchResultDTO> StartAsync(User user, VerificationPurposeEnum purpose);
        Task<VerificationDispatchResultDTO> ResendSmsAsync(Guid challengeId);
        Task<VerificationConfirmationResultDTO> ConfirmAsync(Guid challengeId, string code);
        Task<Guid> RequestWhatsAppAsync(Guid challengeId);
        Task<WhatsAppPreparedMessageDTO> PrepareWhatsAppAsync(Guid requestId, Guid adminUserId);
        Task ConfirmWhatsAppSentAsync(Guid requestId, Guid adminUserId);
    }
}
