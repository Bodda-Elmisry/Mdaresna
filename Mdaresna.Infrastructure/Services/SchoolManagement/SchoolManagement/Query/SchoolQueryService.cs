using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Query;
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
    public class SchoolQueryService : BaseQueryService<School>, ISchoolQueryService
    {
        private readonly IBaseQueryRepository<School> queryRepository;
        private readonly IBaseSharedRepository<School> sharedRepository;
        private readonly ISchoolQueryRepository schoolQueryRepository;

        public SchoolQueryService(IBaseQueryRepository<School> queryRepository,
            IBaseSharedRepository<School> sharedRepository,
            ISchoolQueryRepository schoolQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.schoolQueryRepository = schoolQueryRepository;
        }

        public async Task<SchoolResultDTO?> GetSchoolById(Guid schoolId)
        {
            return await schoolQueryRepository.GetSchoolById(schoolId);
        }

        public async Task<IEnumerable<SchoolResultDTO>> GetSchoolsList(string? name, bool? active, Guid? schoolTypeId, Guid? coinTypeId, Guid? adminId, int pageNumber)
        {
            return await schoolQueryRepository.GetSchoolsList(name, active, schoolTypeId, coinTypeId, adminId, pageNumber);
        }

        public async Task<IEnumerable<SchoolResultDTO>> GetUserAdminSchools(Guid userId)
        {
            return await schoolQueryRepository.GetUserAdminSchools(userId);
        }

        public async Task<IEnumerable<SchoolResultDTO>> GetUserSchools(Guid userId)
        {
            return await schoolQueryRepository.GetUserSchools(userId);
        }













    }
}
