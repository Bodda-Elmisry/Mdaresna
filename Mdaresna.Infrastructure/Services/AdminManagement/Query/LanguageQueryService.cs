using Mdaresna.Doamin.Models.AdminManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.AdminManagement.Query;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.AdminManagement.Query
{
    public class LanguageQueryService : BaseQueryService<Language>, ILanguageQueryService
    {
        private readonly IBaseQueryRepository<Language> queryRepository;
        private readonly IBaseSharedRepository<Language> sharedRepository;

        public LanguageQueryService(IBaseQueryRepository<Language> queryRepository, 
            IBaseSharedRepository<Language> sharedRepository) : 
            base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }


    }
}
