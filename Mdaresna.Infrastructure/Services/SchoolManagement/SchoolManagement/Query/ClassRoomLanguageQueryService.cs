using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Query
{
    public class ClassRoomLanguageQueryService : BaseQueryService<ClassRoomLanguage>, IClassRoomLanguageQueryService
    {
        private readonly IBaseQueryRepository<ClassRoomLanguage> queryRepository;
        private readonly IBaseSharedRepository<ClassRoomLanguage> sharedRepository;

        public ClassRoomLanguageQueryService(IBaseQueryRepository<ClassRoomLanguage> queryRepository,
            IBaseSharedRepository<ClassRoomLanguage> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
