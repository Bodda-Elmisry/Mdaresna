using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.UserManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.UserManagement.Query
{
    public class RelationTypeQueryService : BaseQueryService<RelationType>, IRelationTypeQueryService
    {
        private readonly IBaseQueryRepository<RelationType> queryRepository;
        private readonly IBaseSharedRepository<RelationType> sharedRepository;

        public RelationTypeQueryService(IBaseQueryRepository<RelationType> queryRepository,
            IBaseSharedRepository<RelationType> sharedRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.queryRepository = queryRepository;
            this.sharedRepository = sharedRepository;
        }
    }
}
