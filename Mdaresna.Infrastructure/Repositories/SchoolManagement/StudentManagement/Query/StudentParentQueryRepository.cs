using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Helpers;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.StudentManagement.Query
{
    public class StudentParentQueryRepository : BaseQueryRepository<StudentParent>, IStudentParentQueryRepository
    {
        private readonly AppDbContext context;

        public StudentParentQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<StudentParentResultDTO>> GetParentStudentsAsync(Guid parentId, Guid? relationId)
        {
            var query = GetQuery();
            query = query.Where(p => p.ParentId == parentId);

            if (relationId != null && relationId != Guid.Empty)
                query = query.Where(p => p.RelationId == relationId);

            return await query.ToListAsync();

        }

        public async Task<IEnumerable<StudentParentResultDTO>> GetstudentParentsAsync(Guid studentId, Guid? relationId)
        {
            var query = GetQuery();
            query = query.Where(p => p.StudentId == studentId);

            if(relationId != null && relationId != Guid.Empty)
                query = query.Where(p=> p.RelationId == relationId);

            return await query.ToListAsync();

        }

        public async Task<StudentParentResultDTO?> GetstudentParentAsync(Guid parentId, Guid studentId)
        {
            try
            {
                var query = GetQuery();
                //query = query.Where(p => p.StudentId == studentId && p.ParentId == parentId);
                //var result = await query.FirstOrDefaultAsync();
                //Dispose();
                return await query.FirstOrDefaultAsync(p => p.StudentId == studentId && p.ParentId == parentId);
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        public async Task<StudentParent?> GetstudentParentByIdAsync(Guid parentId, Guid studentId)
        {
            return await context.StudentParents.FirstOrDefaultAsync(p => p.StudentId == studentId && p.ParentId == parentId);
        }

        private IQueryable<StudentParentResultDTO> GetQuery()
        {
            return context.StudentParents
                        .Include(p => p.Parent).Include(p => p.Student).Include(p => p.Relation)
                        .Select(p=> new StudentParentResultDTO
                        {
                            ParentId = p.ParentId,
                            ParentName = $"{p.Parent.FirstName} {p.Parent.MiddelName} {p.Parent.LastName}",
                            ParentImage = !string.IsNullOrEmpty(p.Parent.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{p.Parent.ImageUrl.Replace("\\", "/")}" : "",
                            StudentId = p.StudentId,
                            StudentName = $"{p.Student.FirstName} {p.Student.MiddelName} {p.Student.LastName}",
                            StudentImage = !string.IsNullOrEmpty(p.Student.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{p.Student.ImageUrl.Replace("\\", "/")}" : "",
                            RelationId = p.RelationId,
                            RelationName= p.Relation.Name
                        });
        }



        












    }
}
