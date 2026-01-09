using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Command;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Command
{
    public class SchoolPostReportCommandRepository : BaseCommandRepository<SchoolPostReport>, ISchoolPostReportCommandRepository
    {
        private readonly AppDbContext context;
        private readonly IBaseCommandBulkRepository<SchoolPostReport> baseCommandBulkRepository;

        public SchoolPostReportCommandRepository(AppDbContext context,
                                                 IBaseCommandBulkRepository<SchoolPostReport> baseCommandBulkRepository) : base(context)
        {
            this.context = context;
            this.baseCommandBulkRepository = baseCommandBulkRepository;
        }

        public async Task<bool> DeletePostReportsAsync(Guid postId)
        {
            try
            {
                var reports = await context.SchoolPostReports
                    .AsNoTracking()
                    .Where(r => r.PostId == postId)
                    .ToListAsync();
                if (reports == null || reports.Count == 0)
                {
                    return true;
                }

                return await baseCommandBulkRepository.DeleteBulk(reports);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
