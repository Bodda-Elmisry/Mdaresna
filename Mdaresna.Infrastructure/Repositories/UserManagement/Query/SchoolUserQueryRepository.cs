using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.UserManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.UserManagement.Query
{
    public class SchoolUserQueryRepository : BaseQueryRepository<SchoolUser>, ISchoolUserQueryRepository
    {
       public SchoolUserQueryRepository(AppDbContext context) : base(context)
        {
        }
    }
}
