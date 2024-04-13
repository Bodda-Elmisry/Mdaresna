using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SettingsManagement.Query
{
    public interface ISMSProviderQueryService : IBaseQueryService<SMSProvider>
    {
        Task<IEnumerable<SMSProvider>> GetAllActive();
        Task<SMSProvider> GetFirstActive();
    }
}
