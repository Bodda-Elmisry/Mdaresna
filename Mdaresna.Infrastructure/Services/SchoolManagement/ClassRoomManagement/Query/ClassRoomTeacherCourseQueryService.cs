using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.ClassRoomManagement.Query
{
    public class ClassRoomTeacherCourseQueryService : BaseQueryService<ClassRoomTeacherCourse>, IClassRoomTeacherCourseQueryService
    {
        private readonly IBaseQueryRepository<ClassRoomTeacherCourse> queryRepository;
        private readonly IBaseSharedRepository<ClassRoomTeacherCourse> sharedRepository;

        public ClassRoomTeacherCourseQueryService(IBaseQueryRepository<ClassRoomTeacherCourse> queryRepository,
            IBaseSharedRepository<ClassRoomTeacherCourse> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
