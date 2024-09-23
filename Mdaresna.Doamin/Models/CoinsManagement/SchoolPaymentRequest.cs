using Mdaresna.Doamin.Models.Base;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.Models.UserManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.CoinsManagement
{
    public class SchoolPaymentRequest :AuditBase
    {
        public Guid Id { get; set; }
        public DateTime RequestDate { get; set; }

        public string TransfareCode { get; set; }

        public DateTime TransfareDate { get; set; }

        public decimal TransfareAmount { get; set; }

        public Guid PaymentTypeId { get; set; }

        [ForeignKey(nameof(PaymentTypeId))]
        public PaymentType paymentType { get; set; }

        public Guid SchoolId { get; set; }

        [ForeignKey(nameof(SchoolId))]
        public School School { get; set; }

        public bool? Approvied { get; set; }

        public Guid? ApproviedById { get; set; }

        [ForeignKey(nameof(ApproviedById))]
        public User? ApproviedBy { get; set; }
    }
}
