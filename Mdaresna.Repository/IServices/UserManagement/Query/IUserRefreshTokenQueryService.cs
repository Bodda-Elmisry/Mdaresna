using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Repository.IServices.Base;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.UserManagement.Query
{
    public interface IUserRefreshTokenQueryService : IBaseQueryService<UserRefreshToken>
    {
        Task<UserRefreshToken?> GetByTokenAsync(string token);
    }
}
