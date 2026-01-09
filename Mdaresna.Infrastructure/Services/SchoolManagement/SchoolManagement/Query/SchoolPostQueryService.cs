using Mdaresna.Doamin.DTOs.SchoolManagement;
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
    public class SchoolPostQueryService : BaseQueryService<SchoolPost>, ISchoolPostQueryService
    {
        private readonly IBaseQueryRepository<SchoolPost> queryRepository;
        private readonly IBaseSharedRepository<SchoolPost> sharedRepository;
        private readonly ISchoolPostQueryRepository schoolPostQueryRepository;

        public SchoolPostQueryService(IBaseQueryRepository<SchoolPost> queryRepository,
            IBaseSharedRepository<SchoolPost> sharedRepository,
            ISchoolPostQueryRepository schoolPostQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.schoolPostQueryRepository = schoolPostQueryRepository;
        }

        public async Task<PostResultDTO> GetPostWithImagesAsync(Guid postId)
        {
            return await schoolPostQueryRepository.GetPostWithImagesAsync(postId);
        }

        public async Task<IEnumerable<SchoolPostReportsCountResultDTO>> GetPostsWithReportsCountAsync(string? schoolName, int? minReportsCount, int? maxReportsCount, int pageNumber)
        {
            return await schoolPostQueryRepository.GetPostsWithReportsCountAsync(schoolName, minReportsCount, maxReportsCount, pageNumber);
        }

        public async Task<IEnumerable<PostResultDTO>> GetSchoolPostesWithImagesAsync(Guid schoolId, string searchText, int pageNumber)
        {
            return await schoolPostQueryRepository.GetSchoolPostesWithImagesAsync(schoolId, searchText, pageNumber);
        }
    }
}
