using Mdaresna.DTOs.Common;

namespace Mdaresna.DTOs.CoinsManagementDTO
{
    public class ChangeTypeActivationDTO : PaymentTypeIdDTO
    {
        public bool IsActive { get; set; }
    }
}
