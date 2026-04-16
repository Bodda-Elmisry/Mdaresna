using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.UserManagement.Command;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Infrastructure.Repositories.UserManagement.Command
{
    public class UserReportCommandRepository : BaseCommandRepository<UserReport>, IUserReportCommandRepository
    {
        private readonly AppDbContext context;
        private readonly IBaseCommandBulkRepository<UserReport> baseCommandBulkRepository;

        public UserReportCommandRepository(AppDbContext context,
                                           IBaseCommandBulkRepository<UserReport> baseCommandBulkRepository) : base(context)
        {
            this.context = context;
            this.baseCommandBulkRepository = baseCommandBulkRepository;
        }

        public async Task<bool> DeleteUserReportsAsync(Guid reportedUserId)
        {
            try
            {
                var reports = await context.UserReports
                    .AsNoTracking()
                    .Where(r => r.ReportedUserId == reportedUserId)
                    .ToListAsync();

                if (reports == null || reports.Count == 0)
                {
                    return true;
                }

                return await baseCommandBulkRepository.DeleteBulk(reports);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
