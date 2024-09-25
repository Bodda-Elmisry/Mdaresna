using Mdaresna.DTOs.Common;

namespace Mdaresna.DTOs.CoinsManagementDTO
{
    public class ApprovePaymentRequestDTO : PaymentRequestIdDTO
    {
        public decimal PaymentAmount { get; set; }
        public DateTime PaymentDate { get; set; }
        //public string FromId { get; set; } //schoolid
        //public string FromName { get; set; } //school name
        //public string FromType { get; set; } //School
        public string ToId { get; set; } //AprovedId
        public string ToName { get; set; } //AprovedName
        //public string ToType { get; set; } //AppManager
    }
}
