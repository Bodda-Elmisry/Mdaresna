using Mdaresna.Doamin.Models.AdminManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
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
        private readonly IClassRoomLanguageQueryRepository classRoomLanguageQueryRepository;

        public ClassRoomLanguageQueryService(IBaseQueryRepository<ClassRoomLanguage> queryRepository,
            IBaseSharedRepository<ClassRoomLanguage> sharedRepository,
            IClassRoomLanguageQueryRepository classRoomLanguageQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.classRoomLanguageQueryRepository = classRoomLanguageQueryRepository;
        }

        public async Task<IEnumerable<School>> GetLanguageSchools(Guid LnaguageId)
        {
            return await classRoomLanguageQueryRepository.GetLanguageSchools(LnaguageId);
        }

        public async Task<IEnumerable<Language>> GetSchoolLanguages(Guid SchoolId)
        {
            return await classRoomLanguageQueryRepository.GetSchoolLanguages(SchoolId);

        }
    }
}
