using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Repository.IServices.Base;

using Mdaresna.Doamin.DTOs.SettingsManagement;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SettingsManagement.Query
{
    public interface ISMSLogQueryService : IBaseQueryService<SMSLog>
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
