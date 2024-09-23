namespace Mdaresna.DTOs.CoinsManagementDTO
{
    public class CreateSchoolPaymentRequestDTO
    {
        public DateTime RequestDate { get; set; }
        public string TransfareCode { get; set; }
        public DateTime TransfareDate { get; set; }
        public decimal TransfareAmount { get; set; }
        public Guid PaymentTypeId { get; set; }
        public Guid SchoolId { get; set; }
        public bool? Approvied { get; set; }
        public Guid? ApproviedById { get; set; }
    }
}
