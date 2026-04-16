using Mdaresna.Doamin.DTOs.UserManagement;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Repository.IRepositories.Base;

namespace Mdaresna.Repository.IRepositories.UserManagement.Query
{
    public interface IUserReportQueryRepository : IBaseQueryRepository<UserReport>
    {
        Task<IEnumerable<UserReportResultDTO>> GetUserReportsAsync(Guid reportedUserId);
        Task<IEnumerable<UserReportsCountResultDTO>> GetUsersWithReportsCountAsync(string? userName, int? minReportsCount, int? maxReportsCount, int pageNumber);
    }
}
