using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.UserManagement.Query;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.UserManagement.Query
{
    public class UserRefreshTokenQueryRepository : BaseQueryRepository<UserRefreshToken>, IUserRefreshTokenQueryRepository
    {
        public UserRefreshTokenQueryRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<UserRefreshToken?> GetByTokenAsync(string token)
        {
            return await context.UserRefreshTokens
                .FirstOrDefaultAsync(t => t.Token == token && t.IsRevoked == false && t.Deleted == false);
        }
    }
}
