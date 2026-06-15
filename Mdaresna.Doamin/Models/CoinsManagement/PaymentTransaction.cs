using Mdaresna.Doamin.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mdaresna.Doamin.Models.CoinsManagement
{
    public class PaymentTransaction :AuditBase
    {
        public Guid Id { get; set; }
        public DateTime PaymentDate { get; set; }

        public decimal Amount { get; set; }

        public Guid PaymentTypeId { get; set; }

        [ForeignKey(nameof(PaymentTypeId))]
        public PaymentType paymentType { get; set; }

        public string FromId { get; set; }

        public string ToId { get; set; }

        public Guid CoinTypeId { get; set; }

        [ForeignKey(nameof(CoinTypeId))]
        public CoinType CoinType { get; set; }

        public Guid? SchoolRequestId { get; set; }

        [ForeignKey(nameof(SchoolRequestId))]
        public SchoolPaymentRequest? SchoolPaymentRequest { get; set; }
    }
}
