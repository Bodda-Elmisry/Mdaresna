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
    public class ClassRoomExamQueryService : BaseQueryService<ClassRoomExam>, IClassRoomExamQueryService
    {
        private readonly IBaseQueryRepository<ClassRoomExam> queryRepository;
        private readonly IBaseSharedRepository<ClassRoomExam> sharedRepository;

        public ClassRoomExamQueryService(IBaseQueryRepository<ClassRoomExam> queryRepository,
            IBaseSharedRepository<ClassRoomExam> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
