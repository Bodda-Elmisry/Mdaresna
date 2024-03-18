using Mdaresna.Doamin.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement
{
    public class SchoolYearMonth: BaseModel
    {
        public string? Description { get; set; }
       
        public bool IsActive { get; set; }

        public Guid YearId { get; set; }

        [ForeignKey(nameof(YearId))]
        public SchoolYear Year { get; set; }
    }
}
