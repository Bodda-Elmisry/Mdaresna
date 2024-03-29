using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Command
{
    public class SchoolYearCommandRepository : BaseCommandRepository<SchoolYear>, ISchoolYearCommandRepository
    {
        public SchoolYearCommandRepository(AppDbContext context) : base(context)
        {
        }
    }
}
