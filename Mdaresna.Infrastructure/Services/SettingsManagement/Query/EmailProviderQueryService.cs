using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.SettingsManagement.Query;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SettingsManagement.Query
{
    internal class EmailProviderQueryService : BaseQueryService<EmailProvider>, IEmailProviderQueryService
    {
        IBaseQueryRepository<EmailProvider> queryRepository;
        IBaseSharedRepository<EmailProvider> sharedRepository;
        public EmailProviderQueryService(IBaseQueryRepository<EmailProvider> queryRepository, 
            IBaseSharedRepository<EmailProvider> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
