using Mdaresna.Doamin.Models.AdminManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.AdminManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.AdminManagement.Command
{
    public class LanguageCommandRepository : BaseCommandRepository<Language>, ILanguageCommandRepository
    {
        private readonly AppDbContext context;

        public LanguageCommandRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }
    }
}
