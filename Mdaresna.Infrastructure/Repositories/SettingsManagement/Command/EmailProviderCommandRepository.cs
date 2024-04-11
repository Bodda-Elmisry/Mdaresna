using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SettingsManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SettingsManagement.Command
{
    public class EmailProviderCommandRepository : BaseCommandRepository<EmailProvider>, 
                                                    IEmailProviderCommandRepository
    {
        public EmailProviderCommandRepository(AppDbContext context) : base(context)
        {
        }
    }
}
