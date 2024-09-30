using Mdaresna.DTOs.Common;

namespace Mdaresna.DTOs.CoinsManagementDTO
{
    public class RejectPaymentRequestDTO : PaymentRequestIdDTO
    {
        public Guid RejectedById { get; set; }
    }
}
