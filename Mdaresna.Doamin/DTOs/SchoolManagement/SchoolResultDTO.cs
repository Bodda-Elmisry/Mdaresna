using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.Models.UserManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.SchoolManagement
{
    public class SchoolResultDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string About { get; set; }
        public string Vesion { get; set; }
        public bool? Active { get; set; }
        public string ImageUrl { get; set; }
        public Guid SchoolTypeId { get; set; }
        public string SchoolTypeName { get; set; }
        public Guid? CoinTypeId { get; set; }
        public string CoinTypeName { get; set; }
        public int AvailableCoins { get; set; }
        public Guid SchoolAdminId { get; set; }
        public string SchoolAdminName { get; set; }
    }
}
