using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Query
{
    public class SchoolContactQueryRepository : BaseQueryRepository<SchoolContact>, ISchoolContactQueryRepository
    {
        private readonly AppDbContext context;

        public SchoolContactQueryRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<SchoolContactResultDTO>> GetSchoolContacts(Guid schoolId)
        {
            var result = await context.SchoolContacts.Select(c =>
                                                                new SchoolContactResultDTO
                                                                {
                                                                    SchoolId = c.SchoolId,
                                                                    Id = c.Id,
                                                                    ContactTypeId = c.ContactTypeId,
                                                                    SchoolContactValue = c.Value,
                                                                    TypeDescription = c.ContactType.Description,
                                                                    TypeIcon = c.ContactType.IconUrl,
                                                                    TypeName = c.ContactType.Name
                                                                })
                                        .Where(c => c.SchoolId == schoolId).ToListAsync();

            return result;
        }



    }
}
