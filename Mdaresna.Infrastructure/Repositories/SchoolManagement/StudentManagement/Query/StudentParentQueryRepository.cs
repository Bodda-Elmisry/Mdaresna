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

        public async Task<IEnumerable<ParentStudentResultDTO>> GetParentStudentsAsync(Guid parentId, Guid? relationId)
        {
            var query = GetParentStudentsQuery();
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
                            PhoneNumber = p.Parent.PhoneNumber,
                            StudentId = p.StudentId,
                            StudentName = $"{p.Student.FirstName} {p.Student.MiddelName} {p.Student.LastName}",
                            StudentImage = !string.IsNullOrEmpty(p.Student.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{p.Student.ImageUrl.Replace("\\", "/")}" : "",
                            RelationId = p.RelationId,
                            RelationName= p.Relation.Name
                        });
        }

        private IQueryable<ParentStudentResultDTO> GetParentStudentsQuery()
        {
             var result = from sp in context.StudentParents
                                join s in context.Students on sp.StudentId equals s.Id
                                join sc in context.Schools on s.SchoolId equals sc.Id
                                join cr in context.ClassRooms on s.ClassRoomId equals cr.Id
                                join sg in context.SchoolGrades on cr.GradeId equals sg.Id
                          where s.IsPayed == true && s.Active == true
                                select new ParentStudentResultDTO
                                {
                                    StudentId = s.Id,
                                    StudentCode = s.Code,
                                    StudentName = s.FirstName + " " + s.MiddelName + " " + s.LastName,
                                    ImageUrl = !string.IsNullOrEmpty(s.ImageUrl) ? $"{SettingsHelper.GetAppUrl()}/{s.ImageUrl.Replace("\\", "/")}" : "",
                                    SchoolId = sc.Id,
                                    SchoolName = sc.Name,
                                    CalssroomId = cr.Id,
                                    ClassroomName = cr.Name,
                                    WCSUrl = cr.WCSUrl,
                                    GradeId = sg.Id,
                                    GradeName = sg.Name,
                                    ParentId = sp.ParentId,
                                    RelationId=sp.RelationId
                                };

            var querystring = result.ToQueryString();

            return result;
        }



        












    }
}
