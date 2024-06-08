using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.Models.AdminManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.ClassRoom
{
    public class ClassRoomHelpDataDTO
    {
        public IEnumerable<DropDownDTO> SchoolGrades { get; set; }
        public IEnumerable<DropDownDTO> SchoolLanguages { get; set; }
    }
}
