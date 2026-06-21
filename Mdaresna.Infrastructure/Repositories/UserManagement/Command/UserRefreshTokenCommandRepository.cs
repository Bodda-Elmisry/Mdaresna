using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.UserManagement.Command;

namespace Mdaresna.Infrastructure.Repositories.UserManagement.Command
{
    public class UserRefreshTokenCommandRepository : BaseCommandRepository<UserRefreshToken>, IUserRefreshTokenCommandRepository
    {
        public UserRefreshTokenCommandRepository(AppDbContext context) : base(context)
        {
        }
    }
}
