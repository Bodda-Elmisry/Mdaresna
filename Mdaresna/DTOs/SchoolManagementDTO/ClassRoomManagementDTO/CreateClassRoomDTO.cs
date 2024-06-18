using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.Models.UserManagement;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mdaresna.DTOs.SchoolManagementDTO.ClassRoomManagementDTO
{
    public class CreateClassRoomDTO
    {
        public CreateClassRoomDTO()
        {
            maxOfStudents = 30;
            Active = true;
        }

        public string Name { get; set; }

        public int maxOfStudents { get; set; }

        public Guid SupervisorId { get; set; }

        public bool Active { get; set; }

        /// <summary>
        /// WCS :- Weekly Course Schedule
        /// </summary>
        public string WCSUrl { get; set; }

        public Guid SchoolId { get; set; }

        public Guid LanguageId { get; set; }

        public Guid GradeId { get; set; }

        public int Gender { get; set; }

    }
}
