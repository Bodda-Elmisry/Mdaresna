using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Repository.IRepositories.Base;

using Mdaresna.Doamin.DTOs.SettingsManagement;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.SettingsManagement.Query
{
    public interface ISMSLogQueryRepository : IBaseQueryRepository<SMSLog>
    {
        Task<IEnumerable<SMSLogResultDTO>> GetLogsList(DateTime? fromDate,
                                                       DateTime? toDate,
                                                       string? phoneNumber,
                                                       Guid? smsProviderId,
                                                       bool? isSuccess,
                                                       string? message,
                                                       string? response,
                                                       int pageNumber);
    }
}
