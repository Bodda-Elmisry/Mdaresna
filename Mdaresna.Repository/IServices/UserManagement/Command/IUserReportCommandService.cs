using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Repository.IServices.Base;

namespace Mdaresna.Repository.IServices.UserManagement.Command
{
    public interface IUserReportCommandService : IBaseCommandService<UserReport>
    {
        Task<bool> DeleteUserReportsByReportedUserIdAsync(Guid reportedUserId);
    }
}
