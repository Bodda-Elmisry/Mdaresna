namespace Mdaresna.DTOs.CoinsManagementDTO
{
    public class UpdateSchoolPaymentRequestDTO
    {
        public Guid Id { get; set; }
        public string TransfareCode { get; set; }
        public DateTime TransfareDate { get; set; }
        public decimal TransfareAmount { get; set; }
        public Guid PaymentTypeId { get; set; }
    }
}
