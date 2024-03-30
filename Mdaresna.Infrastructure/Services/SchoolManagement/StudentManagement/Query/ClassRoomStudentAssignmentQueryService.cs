using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Query
{
    public class ClassRoomStudentAssignmentQueryService : BaseQueryService<ClassRoomStudentAssignment>, IClassRoomStudentAssignmentQueryService
    {
        private readonly IBaseQueryRepository<ClassRoomStudentAssignment> queryRepository;
        private readonly IBaseSharedRepository<ClassRoomStudentAssignment> sharedRepository;

        public ClassRoomStudentAssignmentQueryService(IBaseQueryRepository<ClassRoomStudentAssignment> queryRepository,
            IBaseSharedRepository<ClassRoomStudentAssignment> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
