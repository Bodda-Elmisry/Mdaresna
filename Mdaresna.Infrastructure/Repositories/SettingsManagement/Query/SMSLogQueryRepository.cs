using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.SettingsManagement;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SettingsManagement.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SettingsManagement.Query
{
    public class SMSLogQueryRepository : BaseQueryRepository<SMSLog>, ISMSLogQueryRepository
    {
        private readonly AppDbContext context;
        private readonly AppSettingDTO appSettings;

        public SMSLogQueryRepository(AppDbContext context, IOptions<AppSettingDTO> appSettings) : base(context)
        {
            this.context = context;
            this.appSettings = appSettings.Value;
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
            var resolvedPageNumber = pageNumber > 0 ? pageNumber : 1;
            var pageSize = appSettings.PageSize != null ? appSettings.PageSize.Value : 30;

            var query = context.SMSLogs.Where(l => l.Deleted == false);

            if (fromDate != null && toDate != null)
                query = query.Where(l => l.CreateDate >= fromDate && l.CreateDate <= toDate);
            else if (fromDate != null)
                query = query.Where(l => l.CreateDate >= fromDate);
            else if (toDate != null)
                query = query.Where(l => l.CreateDate <= toDate);

            if (!string.IsNullOrEmpty(phoneNumber))
                query = query.Where(l => l.PhoneNumber.Contains(phoneNumber));

            if (smsProviderId != null)
                query = query.Where(l => l.SMSProviderId == smsProviderId);

            if (isSuccess != null)
                query = query.Where(l => l.IsSuccess == isSuccess);

            if (!string.IsNullOrEmpty(message))
                query = query.Where(l => l.Message.Contains(message));

            if (!string.IsNullOrEmpty(response))
                query = query.Where(l => l.Response.Contains(response));

            return await query
                .OrderByDescending(l => l.CreateDate)
                .Skip((resolvedPageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new SMSLogResultDTO
                {
                    Id = l.Id,
                    SMSProviderId = l.SMSProviderId,
                    PhoneNumber = l.PhoneNumber,
                    Message = l.Message,
                    Response = l.Response,
                    IsSuccess = l.IsSuccess,
                    CreateDate = l.CreateDate
                })
                .ToListAsync();
        }
    }
}
