using Mdaresna.Doamin.Models.CoinsManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.CoinsManagement
{
    public class PaymentTransactionResultDTO
    {
        private string FromInfo;
        private string ToInfo;
        public Guid Id { get; set; }
        public DateTime PaymentDate { get; set; }
        public decimal Amount { get; set; }
        public Guid PaymentTypeId { get; set; }
        public string paymentType { get; set; }
        public string FromId {
            get
            {
                var fromParts = this.FromInfo.Split("|||");
                return fromParts.Count() > 2 ? fromParts[1] : string.Empty;
            }
            set
            {
                this.FromInfo = value;
            }
        }
        public string FromName 
        { 
            get 
            {
                var fromParts = this.FromInfo.Split("|||");
                return fromParts.Count() > 2 ? fromParts[fromParts.Count() - 1] : string.Empty;
            }
        }
        public string FromType
        {
            get
            {
                var fromParts = this.FromInfo.Split("|||");
                return fromParts.Count() > 2 ? fromParts[0] : string.Empty;
            }
        }
        public string ToId
        {
            get
            {
                var fromParts = this.ToInfo.Split("|||");
                return fromParts.Count() > 2 ? fromParts[1] : string.Empty;
            }
            set
            {
                this.ToInfo = value;
            }
        }
        public string ToName
        {
            get
            {
                var fromParts = this.ToInfo.Split("|||");
                return fromParts.Count() > 2 ? fromParts[fromParts.Count() - 1] : string.Empty;
            }
        }
        public string ToType
        {
            get
            {
                var fromParts = this.ToInfo.Split("|||");
                return fromParts.Count() > 2 ? fromParts[0] : string.Empty;
            }
        }
        public Guid CoinTypeId { get; set; }
        public string CoinType { get; set; }
        public Guid? SchoolRequestId { get; set; }
        public string? SchoolPaymentRequest { get; set; }

    }
}
