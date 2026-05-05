using Mdaresna.Doamin.DTOs.UserManagement;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.UserManagement.Query;
using Mdaresna.Repository.IServices.UserManagement.Query;

namespace Mdaresna.Infrastructure.Services.UserManagement.Query
{
    public class UserReportQueryService : BaseQueryService<UserReport>, IUserReportQueryService
    {
        private readonly IUserReportQueryRepository userReportQueryRepository;

        public UserReportQueryService(IBaseQueryRepository<UserReport> queryRepository,
                                      IBaseSharedRepository<UserReport> sharedRepository,
                                      IUserReportQueryRepository userReportQueryRepository) : base(queryRepository, sharedRepository)
        {
            this.userReportQueryRepository = userReportQueryRepository;
        }

        public async Task<IEnumerable<UserReportResultDTO>> GetUserReportsAsync(Guid reportedUserId)
        {
            return await userReportQueryRepository.GetUserReportsAsync(reportedUserId);
        }

        public async Task<IEnumerable<UserReportsCountResultDTO>> GetUsersWithReportsCountAsync(Guid? schoolId, string? userName, int? minReportsCount, int? maxReportsCount, int pageNumber)
        {
            return await userReportQueryRepository.GetUsersWithReportsCountAsync(schoolId, userName, minReportsCount, maxReportsCount, pageNumber);
        }
    }
}
