using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SettingsManagement.Command;

namespace Mdaresna.Infrastructure.Repositories.SettingsManagement.Command
{
    public class SMSLogCommandRepository : BaseCommandRepository<SMSLog>, ISMSLogCommandRepository
    {
        public SMSLogCommandRepository(AppDbContext context) : base(context)
        {
        }
    }
}
