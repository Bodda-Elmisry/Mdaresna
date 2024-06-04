using Mdaresna.Doamin.Models.AdminManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.AdminManagement.Command;
using Mdaresna.Repository.IRepositories.AdminManagement.Query;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.AdminManagement.Query
{
    internal class LanguageQueryRepository : BaseQueryRepository<Language>, ILanguageQueryRepository
    {
        private readonly AppDbContext context;

        public LanguageQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }
    }
}
