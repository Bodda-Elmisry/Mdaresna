using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.SchoolManagement
{
    public record class GetSchoolTeachersDTO(
        Guid SchoolId,
        string TeacherName,
        string TeacherPhoneNumber,
        string TeacherEmail
        );
}
