namespace Mdaresna.DTOs.CoinsManagementDTO
{
    public class GetSchoolPaymentRequestsDTO
    {
        public DateTime? RequestDateFrom { get; set; }
        public DateTime? RequestDateTo { get; set; }
        public string? TransfareCode { get; set; }
        public DateTime? TransfareDateFrom { get; set; }
        public DateTime? TransfareDateTo { get; set; }
        public decimal? TransfareAmount { get; set; }
        public Guid? PaymentTypeId { get; set; }
        public Guid? SchoolId { get; set; }
        public bool? Approvied { get; set; }
        public Guid? ApproviedById { get; set; }
    }
}
