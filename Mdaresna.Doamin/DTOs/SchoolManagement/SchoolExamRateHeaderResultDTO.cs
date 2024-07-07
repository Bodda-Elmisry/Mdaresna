using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.SchoolManagement
{
    public class SchoolExamRateHeaderResultDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string? Note { get; set; }
        public decimal Percentage { get; set; }
        public Guid SchoolId { get; set; }
    }
}
