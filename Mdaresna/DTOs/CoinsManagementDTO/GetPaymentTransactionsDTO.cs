namespace Mdaresna.DTOs.CoinsManagementDTO
{
    public class GetPaymentTransactionsDTO
    {

        public DateTime? PaymentDateFrom { get; set; }
        public DateTime? PaymentDateTO { get; set; }
        public decimal? Amount { get; set; }
        public Guid? PaymentTypeId { get; set; }
        public string? FromId { get; set; }
        public string? FromName { get; set; }
        public string? FromType { get; set; }
        public string? ToId { get; set;}
        public string? ToName { get; set;}
        public string? ToType { get; set; }
        public Guid? CoinTypeId { get; set; }
        public Guid? SchoolRequestId { get; set; }

    }
}
