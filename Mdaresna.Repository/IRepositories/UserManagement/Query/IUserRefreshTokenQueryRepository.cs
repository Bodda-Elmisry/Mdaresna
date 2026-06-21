using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Repository.IRepositories.Base;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.UserManagement.Query
{
    public interface IUserRefreshTokenQueryRepository : IBaseQueryRepository<UserRefreshToken>
    {
        Task<UserRefreshToken?> GetByTokenAsync(string token);
    }
}
