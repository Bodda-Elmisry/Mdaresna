using Mdaresna.Doamin.Models.Base;
using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Doamin.Models.UserManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement
{
    public class School : BaseModel
    {
        
        public string About { get; set; }

        public string Vesion { get; set; }

        public bool? Active { get; set; }

        public string ImageUrl { get; set; }

        public Guid SchoolTypeId { get; set; }

        [ForeignKey(nameof(SchoolTypeId))]
        public virtual SchoolType SchoolType { get; set; }

        public Guid? CoinTypeId { get; set; }

        [ForeignKey(nameof(CoinTypeId))]
        public virtual CoinType CoinType { get; set; }

        public int AvailableCoins { get; set; }
        public Guid SchoolAdminId { get; set; }

        [ForeignKey(nameof(SchoolAdminId))]
        public virtual User SchoolAdmin { get; set; }

        public ICollection<SchoolImage> SchoolImages { get; set; }
    }
}
