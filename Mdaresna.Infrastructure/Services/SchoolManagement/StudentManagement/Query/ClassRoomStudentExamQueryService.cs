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
    public class ClassRoomStudentExamQueryService : BaseQueryService<ClassRoomStudentExam>, IClassRoomStudentExamQueryService
    {
        private readonly IBaseQueryRepository<ClassRoomStudentExam> queryRepository;
        private readonly IBaseSharedRepository<ClassRoomStudentExam> sharedRepository;

        public ClassRoomStudentExamQueryService(IBaseQueryRepository<ClassRoomStudentExam> queryRepository,
            IBaseSharedRepository<ClassRoomStudentExam> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
