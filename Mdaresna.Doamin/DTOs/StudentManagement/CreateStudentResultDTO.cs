using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.StudentManagement
{
    public class CreateStudentResultDTO
    {

        public Student? Student { get; set; }
        public int AvailableCoins { get; set; }
    }
}
