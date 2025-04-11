using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.SchoolManagement
{
    public class GetSchoolPostsListDTO
    {
        public Guid SchoolId { get; set; }
        public string? SerachText { get; set; }
        public int PageNumber { get; set; }
    }
}
