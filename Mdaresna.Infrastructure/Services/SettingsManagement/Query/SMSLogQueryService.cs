using Mdaresna.Doamin.DTOs.SettingsManagement;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SettingsManagement.Query;
using Mdaresna.Repository.IServices.SettingsManagement.Query;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SettingsManagement.Query
{
    internal class SMSLogQueryService : BaseQueryService<SMSLog>, ISMSLogQueryService
    {
        IBaseQueryRepository<SMSLog> queryRepository;
        IBaseSharedRepository<SMSLog> sharedRepository;
        private readonly ISMSLogQueryRepository smsLogQueryRepository;

        public SMSLogQueryService(IBaseQueryRepository<SMSLog> queryRepository,
            IBaseSharedRepository<SMSLog> sharedRepository,
            ISMSLogQueryRepository smsLogQueryRepository)
            : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
            this.smsLogQueryRepository = smsLogQueryRepository;
        }

        public async Task<IEnumerable<SMSLogResultDTO>> GetLogsList(DateTime? fromDate,
                                                                    DateTime? toDate,
                                                                    string? phoneNumber,
                                                                    Guid? smsProviderId,
                                                                    bool? isSuccess,
                                                                    string? message,
                                                                    string? response,
                                                                    int pageNumber)
        {
            return await smsLogQueryRepository.GetLogsList(fromDate,
                                                           toDate,
                                                           phoneNumber,
                                                           smsProviderId,
                                                           isSuccess,
                                                           message,
                                                           response,
                                                           pageNumber);
        }
    }
}
