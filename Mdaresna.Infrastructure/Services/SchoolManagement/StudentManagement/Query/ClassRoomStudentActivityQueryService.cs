using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Query
{
    public class ClassRoomStudentActivityQueryService : BaseQueryService<ClassRoomStudentActivity>, IClassRoomStudentActivityQueryService
    {
        private readonly IClassRoomStudentActivityQueryRepository classRoomStudentActivityQueryRepository;

        public ClassRoomStudentActivityQueryService(IBaseQueryRepository<ClassRoomStudentActivity> queryRepository,
            IBaseSharedRepository<ClassRoomStudentActivity> sharedRepository,
            IClassRoomStudentActivityQueryRepository classRoomStudentActivityQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.classRoomStudentActivityQueryRepository = classRoomStudentActivityQueryRepository;
        }

        public async Task<ClassRoomStudentActivity?> GetClassRoomStudentActivityAsync(Guid studentId, Guid ActivityId)
        {
            return await classRoomStudentActivityQueryRepository.GetClassRoomStudentActivityAsync(studentId, ActivityId);
        }

        public async Task<ClassRoomStudentActivityResultDTO?> GetClassRoomStudentActivityViewAsync(Guid studentId, Guid ActivityId)
        {
            return await classRoomStudentActivityQueryRepository.GetClassRoomStudentActivityViewAsync(studentId, ActivityId);
        }

        public async Task<IEnumerable<ClassRoomStudentActivityResultDTO>> GetStudentActivitiesListAsync(Guid? StudentId, Guid? ActivityId, decimal? ResultFrom, decimal? ResultTo, int pageNumber)
        {
            return await classRoomStudentActivityQueryRepository.GetStudentActivitiesListAsync(StudentId, ActivityId, ResultFrom, ResultTo, pageNumber);
        }
    }
}
