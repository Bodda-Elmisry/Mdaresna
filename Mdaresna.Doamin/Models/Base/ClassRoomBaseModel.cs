using Mdaresna.Doamin.Models.Base.Relation;
using System.ComponentModel.DataAnnotations;

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
