using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.StudentManagement.Command
{
    public class ClassRoomStudentActivityCommandRepository : BaseCommandRepository<ClassRoomStudentActivity>, IClassRoomStudentActivityCommandRepository
    {
        public ClassRoomStudentActivityCommandRepository(AppDbContext context) : base(context)
        {
        }
    }
}
