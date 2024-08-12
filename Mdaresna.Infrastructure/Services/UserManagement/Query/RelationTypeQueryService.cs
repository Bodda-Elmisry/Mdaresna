using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Services.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.UserManagement.Query;
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
        private readonly IRelationTypeQueryRepository relationTypeQueryRepository;

        public RelationTypeQueryService(IBaseQueryRepository<RelationType> queryRepository,
            IBaseSharedRepository<RelationType> sharedRepository,
            IRelationTypeQueryRepository relationTypeQueryRepository) 
                : base(queryRepository, sharedRepository)
        {
            this.relationTypeQueryRepository = relationTypeQueryRepository;
        }

        public async Task<RelationType?> GetRelationByNameAsync(string name)
        {
            return await relationTypeQueryRepository.GetRelationByNameAsync(name);
        }
    }
}
