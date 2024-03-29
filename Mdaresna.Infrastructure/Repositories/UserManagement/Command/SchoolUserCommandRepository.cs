using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.UserManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.UserManagement.Command
{
    public class SchoolUserCommandRepository : BaseCommandRepository<SchoolUser>, ISchoolUserCommandRepository
    {
        public SchoolUserCommandRepository(AppDbContext context) : base(context)
        {
        }
    }
}
