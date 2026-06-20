using Mdaresna.Doamin.Enums;
using System;

namespace Mdaresna.Doamin.DTOs.ClassRoomManagement
{
    public class GetSchoolClassesFilteredDTO
    {
        public Guid SchoolId { get; set; }
        public string? Name { get; set; }
        public Guid? LanguageId { get; set; }
        public Guid? GradeId { get; set; }
        public ClassRoomGenderEnum? Gender { get; set; }
    }
}
