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
    public class StudentNoteQueryService : BaseQueryService<StudentNote>, IStudentNoteQueryService
    {
        private readonly IBaseQueryRepository<StudentNote> queryRepository;
        private readonly IBaseSharedRepository<StudentNote> sharedRepository;

        public StudentNoteQueryService(IBaseQueryRepository<StudentNote> queryRepository,
            IBaseSharedRepository<StudentNote> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
