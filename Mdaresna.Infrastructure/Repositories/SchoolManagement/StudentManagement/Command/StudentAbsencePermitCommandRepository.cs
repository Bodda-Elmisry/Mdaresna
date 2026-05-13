using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Command;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.StudentManagement.Command
{
    public class StudentAbsencePermitCommandRepository : BaseCommandRepository<StudentAbsencePermit>, IStudentAbsencePermitCommandRepository
    {
        public StudentAbsencePermitCommandRepository(AppDbContext context) : base(context)
        {
        }
    }
}
