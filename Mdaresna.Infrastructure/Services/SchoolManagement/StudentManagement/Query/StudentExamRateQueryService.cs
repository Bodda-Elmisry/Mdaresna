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
    public class StudentExamRateQueryService : BaseQueryService<StudentExamRate>, IStudentExamRateQueryService
    {
        private readonly IBaseQueryRepository<StudentExamRate> queryRepository;
        private readonly IBaseSharedRepository<StudentExamRate> sharedRepository;

        public StudentExamRateQueryService(IBaseQueryRepository<StudentExamRate> queryRepository,
            IBaseSharedRepository<StudentExamRate> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
