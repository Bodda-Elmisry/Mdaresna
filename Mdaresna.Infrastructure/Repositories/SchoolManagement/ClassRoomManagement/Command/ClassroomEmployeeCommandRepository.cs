using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.ClassRoomManagement.Command
{
    internal class ClassroomEmployeeCommandRepository : BaseCommandRepository<ClassroomEmployee>, IClassroomEmployeeCommandRepository
    {
        public ClassroomEmployeeCommandRepository(AppDbContext context) : base(context)
        {
        }
    }
}
