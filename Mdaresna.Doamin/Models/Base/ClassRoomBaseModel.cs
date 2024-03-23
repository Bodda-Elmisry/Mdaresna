using Mdaresna.Doamin.Models.Base.Relation;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.Models.UserManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.Base
{
    public class ClassRoomBaseModel : ClassRoomSupervisorCourseReltaion
    {
        public Guid Id { get; set; }

        [MaxLength(25)]
        public string WeekDay { get; set; }

        public string Details { get; set; }

        
        public decimal Rate { get; set; }
    }
}
