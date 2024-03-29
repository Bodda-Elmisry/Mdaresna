using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.ClassRoomManagement.Query
{
    public class ClassRoomAssignmentQueryRepository : BaseQueryRepository<ClassRoomAssignment>, IClassRoomAssignmentQueryRepository
    {
       public ClassRoomAssignmentQueryRepository(AppDbContext context) : base(context)
        {
        }
    }
}
