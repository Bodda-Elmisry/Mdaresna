using Mdaresna.Doamin.Models.Base;

namespace Mdaresna.Doamin.Models.CoinsManagement
{
    public class PaymentType : BaseModel
    {
        public bool IsActive { get; set; }

        public string? Note { get; set; }
    }
}
