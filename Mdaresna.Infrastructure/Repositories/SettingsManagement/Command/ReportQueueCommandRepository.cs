using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SettingsManagement.Command;

namespace Mdaresna.Infrastructure.Repositories.SettingsManagement.Command
{
    public class ReportQueueCommandRepository : BaseCommandRepository<ReportQueue>, IReportQueueCommandRepository
    {
        public ReportQueueCommandRepository(AppDbContext context) : base(context)
        {
        }
    }
}
