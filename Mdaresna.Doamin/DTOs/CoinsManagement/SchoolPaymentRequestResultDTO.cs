using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.Models.UserManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.CoinsManagement
{
    public class SchoolPaymentRequestResultDTO
    {
        public Guid Id { get; set; }
        public DateTime RequestDate { get; set; }
        public string TransfareCode { get; set; }
        public DateTime TransfareDate { get; set; }
        public decimal TransfareAmount { get; set; }
        public Guid PaymentTypeId { get; set; }
        public string PaymentTypeName { get; set; }
        public Guid SchoolId { get; set; }
        public string SchoolName { get; set; }
        public bool? Approvied { get; set; }
        public Guid? ApproviedById { get; set; }
        public string? ApproviedByName { get; set; }
    }
}
