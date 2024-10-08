using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.UserManagement.Query
{
    public interface IRelationTypeQueryService : IBaseQueryService<RelationType>
    {
        Task<RelationType?> GetRelationByNameAsync(string name);
    }
}
