using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SettingsManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SettingsManagement.Query
{
    public class SMSProviderQueryRepository : BaseQueryRepository<SMSProvider>, ISMSProviderQueryRepository
    {
        public SMSProviderQueryRepository(AppDbContext context) : base(context)
        {
        }
    }
}
