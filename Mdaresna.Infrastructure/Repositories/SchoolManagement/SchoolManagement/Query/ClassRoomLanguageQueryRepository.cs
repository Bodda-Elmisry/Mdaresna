using Mdaresna.Doamin.Models.AdminManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Query
{
    public class ClassRoomLanguageQueryRepository : BaseQueryRepository<ClassRoomLanguage>, IClassRoomLanguageQueryRepository
    {
        private readonly AppDbContext context;

        public ClassRoomLanguageQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<School>> GetLanguageSchools(Guid LnaguageId)
        {
            var result = await context.ClassRoomLanguages
                        .Where(c => c.LanguageId == LnaguageId)
                        .Select(c => c.School).ToListAsync();
            return result;

        }

        public async Task<IEnumerable<Language>> GetSchoolLanguages(Guid SchoolId)
        {
            var result = await context.ClassRoomLanguages
                        .Where(c => c.SchoolId == SchoolId)
                        .Select(c => c.Language).ToListAsync();
            return result;

        }





    }
}
