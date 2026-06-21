using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.UserManagement.Query;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.UserManagement.Query;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.UserManagement.Query
{
    public class UserRefreshTokenQueryService : IBaseQueryService<UserRefreshToken>, IUserRefreshTokenQueryService
    {
        private readonly IUserRefreshTokenQueryRepository queryRepository;

        public UserRefreshTokenQueryService(IUserRefreshTokenQueryRepository queryRepository)
        {
            this.queryRepository = queryRepository;
        }

        public async Task<IEnumerable<UserRefreshToken>> GetAllAsync()
        {
            return await queryRepository.GetAllAsync();
        }

        public async Task<UserRefreshToken> GetByIdAsync(Guid id)
        {
            return await queryRepository.GetByIdAsync(id);
        }

        public async Task<UserRefreshToken?> GetByTokenAsync(string token)
        {
            return await queryRepository.GetByTokenAsync(token);
        }
    }
}
