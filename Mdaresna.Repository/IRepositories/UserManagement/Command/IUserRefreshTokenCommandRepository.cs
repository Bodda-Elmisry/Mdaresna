using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Repository.IRepositories.Base;

namespace Mdaresna.Repository.IRepositories.UserManagement.Command
{
    public interface IUserRefreshTokenCommandRepository : IBaseCommandRepository<UserRefreshToken>
    {
    }
}
