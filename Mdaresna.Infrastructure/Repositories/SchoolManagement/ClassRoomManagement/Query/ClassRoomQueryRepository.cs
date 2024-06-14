using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.ClassRoomManagement.Query
{
    public class ClassRoomQueryRepository : BaseQueryRepository<ClassRoom>, IClassRoomQueryRepository
    {
        private readonly AppDbContext context;

        public ClassRoomQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<ClassRoom>> GetBySchoolIdAsync(Guid SchoolId)
        {
            return await context.ClassRooms
                            .Include(c=> c.Grade).Include(c=> c.Language).Include(c=> c.Supervisor)
                            .Where(c=> c.SchoolId == SchoolId).ToListAsync();
        }

        public async Task<IEnumerable<ClassRoom>> GetBySchoolIdAndSupervisorIdAsync(Guid SchoolId, Guid SupervisorId)
        {
            return await context.ClassRooms.Where(c => c.SchoolId == SchoolId && c.SupervisorId == SupervisorId).ToListAsync();
        }
    }
}
