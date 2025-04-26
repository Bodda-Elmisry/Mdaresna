using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SettingsManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SettingsManagement.Query
{
    public class SMSProviderQueryRepository : BaseQueryRepository<SMSProvider>, ISMSProviderQueryRepository
    {
        private readonly AppDbContext context;

        public SMSProviderQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<SMSProvider>> GetAllActive()
        {
            return await context.SMSProviders.Where(s => s.Active == true && s.Deleted == false).ToListAsync();
        }

        public async Task<SMSProvider?> GetFirstActive()
        {
            return await context.SMSProviders.FirstOrDefaultAsync(s => s.Active == true && s.Deleted == false);
        }
    }
}
