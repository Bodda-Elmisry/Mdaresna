using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using System;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Command
{
    public class SchoolPostReportCommandService : IBaseCommandService<SchoolPostReport>, ISchoolPostReportCommandService
    {
        private readonly IBaseCommandRepository<SchoolPostReport> commandRepository;
        private readonly IBaseSharedRepository<SchoolPostReport> sharedRepository;
        private readonly ISchoolPostReportCommandRepository schoolPostReportCommandRepository;

        public SchoolPostReportCommandService(IBaseCommandRepository<SchoolPostReport> commandRepository,
            IBaseSharedRepository<SchoolPostReport> sharedRepository,
            ISchoolPostReportCommandRepository schoolPostReportCommandRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
            this.schoolPostReportCommandRepository = schoolPostReportCommandRepository;
        }

        public bool Create(SchoolPostReport entity)
        {
            entity.Id = DataGenerationHelper.GenerateRowId();
            entity.CreateDate = DateTime.Now;
            entity.LastModifyDate = DateTime.Now;
            return commandRepository.Create(entity);
        }

        public async Task<bool> DeleteAsync(SchoolPostReport entity)
        {
            entity = await sharedRepository.GetAsync(entity.Id);
            return commandRepository.Delete(entity);
        }

        public bool Update(SchoolPostReport entity)
        {
            entity.LastModifyDate = DateTime.Now;
            return commandRepository.Update(entity);
        }

        public async Task<bool> DeletePostReportsByPostIdAsync(Guid postId)
        {
            return await schoolPostReportCommandRepository.DeletePostReportsAsync(postId);
        }
    }
}
