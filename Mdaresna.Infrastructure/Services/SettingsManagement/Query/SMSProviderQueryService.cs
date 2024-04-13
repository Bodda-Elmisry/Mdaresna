using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SettingsManagement.Query;
using Mdaresna.Repository.IServices.SettingsManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SettingsManagement.Query
{
    public class SMSProviderQueryService : BaseQueryService<SMSProvider>, ISMSProviderQueryService
    {
        IBaseQueryRepository<SMSProvider> queryRepository;
        IBaseSharedRepository<SMSProvider> sharedRepository;
        private readonly ISMSProviderQueryRepository sMSProviderQueryRepository;

        public SMSProviderQueryService(IBaseQueryRepository<SMSProvider> queryRepository, 
            IBaseSharedRepository<SMSProvider> sharedRepository,
            ISMSProviderQueryRepository sMSProviderQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.sMSProviderQueryRepository = sMSProviderQueryRepository;
        }

        public async Task<IEnumerable<SMSProvider>> GetAllActive()
        {
            return await sMSProviderQueryRepository.GetAllAsync();
        }

        public async Task<SMSProvider> GetFirstActive()
        {
            return await sMSProviderQueryRepository.GetFirstActive();
        }
    }
}
