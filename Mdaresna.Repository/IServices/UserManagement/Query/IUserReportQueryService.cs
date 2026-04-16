using Mdaresna.Doamin.DTOs.UserManagement;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Repository.IServices.Base;

namespace Mdaresna.Repository.IServices.UserManagement.Query
{
    public interface IUserReportQueryService : IBaseQueryService<UserReport>
    {
        Task<IEnumerable<UserReportResultDTO>> GetUserReportsAsync(Guid reportedUserId);
        Task<IEnumerable<UserReportsCountResultDTO>> GetUsersWithReportsCountAsync(string? userName, int? minReportsCount, int? maxReportsCount, int pageNumber);
    }
}
