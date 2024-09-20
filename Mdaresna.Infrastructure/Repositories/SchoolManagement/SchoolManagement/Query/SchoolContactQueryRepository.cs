using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Helpers;
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
            var result = await context.SchoolContacts.Include(c=> c.ContactType).Select(c =>
                                                                new SchoolContactResultDTO
                                                                {
                                                                    SchoolId = c.SchoolId,
                                                                    Id = c.Id,
                                                                    ContactTypeId = c.ContactTypeId,
                                                                    SchoolContactValue = c.Value,
                                                                    TypeDescription = c.ContactType.Description,
                                                                    //TypeIcon = this.GetTypeIconeURL(c.ContactType.IconUrl),
                                                                    TypeIcon = !string.IsNullOrEmpty(c.ContactType.IconUrl) ? $"{SettingsHelper.GetAppUrl()}/{c.ContactType.IconUrl.Replace("\\", "/")}" : string.Empty,
                                                                    TypeName = c.ContactType.Name
                                                                })
                                        .Where(c => c.SchoolId == schoolId).ToListAsync();

            return result;
        }



        public async Task<SchoolContactResultDTO?> GetSchoolContactById(Guid Id)
        {
            var result = await context.SchoolContacts.FirstOrDefaultAsync(c => c.Id == Id);

            return result == null ? null : new SchoolContactResultDTO
            {
                SchoolId = result.SchoolId,
                Id = result.Id,
                ContactTypeId = result.ContactTypeId,
                SchoolContactValue = result.Value,
                TypeDescription = result.ContactType.Description,
                TypeIcon = this.GetTypeIconeURL(result.ContactType.IconUrl),
                TypeName = result.ContactType.Name
            };
        }

        private string? GetTypeIconeURL(string? url)
        {
            return !string.IsNullOrEmpty(url) ? $"{SettingsHelper.GetAppUrl()}/{url.Replace("\\", "/")}" : string.Empty;
        }



    }
}
