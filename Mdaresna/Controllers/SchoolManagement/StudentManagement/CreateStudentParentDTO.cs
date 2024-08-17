namespace Mdaresna.Controllers.SchoolManagement.StudentManagement
{
    public class CreateStudentParentDTO
    {
        public Guid ParentId { get; set; }
        public Guid StudentId { get; set; }
        public Guid RelationId { get; set; }
    }
}
