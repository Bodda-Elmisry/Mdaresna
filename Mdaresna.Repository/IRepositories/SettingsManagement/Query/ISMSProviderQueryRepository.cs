﻿using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.SettingsManagement.Query
{
    public interface ISMSProviderQueryRepository : IBaseQueryRepository<SMSProvider>
    {
        Task<IEnumerable<SMSProvider>> GetAllActive();
        Task<SMSProvider?> GetFirstActive();
    }
}
