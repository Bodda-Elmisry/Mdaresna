using Mdaresna.Doamin.DTOs.SchoolManagement;
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
    public class SchoolCourseQueryRepository : BaseQueryRepository<SchoolCourse>, ISchoolCourseQueryRepository
    {
        private readonly AppDbContext context;

        public SchoolCourseQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<SchoolCourseResultDTO>> GetCoursesBySchoolIdAsync(Guid schoolId)
        {
            return await context.SchoolCourses
                        .Where(c => c.SchoolId == schoolId && c.Deleted == false)
                        .Select(s=> new SchoolCourseResultDTO
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Description = s.Description,
                            LanguageId = s.LanguageId,
                            LanguageName = s.Language.Name,
                            SchoolId = s.SchoolId,
                            SchoolName = s.School.Name
                        })
                        .ToListAsync();
        }

        public async Task<IEnumerable<SchoolCourseResultDTO>> GetCoursesBySchoolIdAndLanguageIDAsync(Guid schoolId, Guid languageId)
        {
            return await context.SchoolCourses
                        .Where(c => c.SchoolId == schoolId && c.LanguageId == languageId && c.Deleted == false)
                        .Select(s => new SchoolCourseResultDTO
                        {
                            Id = s.Id,
                            Name = s.Name,
                            Description = s.Description,
                            LanguageId = s.LanguageId,
                            LanguageName = s.Language.Name,
                            SchoolId = s.SchoolId,
                            SchoolName = s.School.Name
                        })

                        .ToListAsync();
        }

        public async Task<SchoolCourseResultDTO?> GetCourseIDAsync(Guid id)
        {
            var schoolCourse = await context.SchoolCourses
                                     .Include(s => s.School).Include(s => s.Language)
                                     .FirstOrDefaultAsync(s => s.Id == id && s.Deleted == false);

            return schoolCourse == null ? null :
                                    new SchoolCourseResultDTO
                                    {
                                        Id = schoolCourse.Id,
                                        Name = schoolCourse.Name,
                                        Description = schoolCourse.Description,
                                        LanguageId = schoolCourse.LanguageId,
                                        LanguageName = schoolCourse.Language.Name,
                                        SchoolId = schoolCourse.SchoolId,
                                        SchoolName = schoolCourse.School.Name
                                    };
        }
    }
}
