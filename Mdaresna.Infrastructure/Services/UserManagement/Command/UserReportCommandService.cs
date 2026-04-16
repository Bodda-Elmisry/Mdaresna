using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.UserManagement.Command;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.UserManagement.Command;

namespace Mdaresna.Infrastructure.Services.UserManagement.Command
{
    public class UserReportCommandService : IBaseCommandService<UserReport>, IUserReportCommandService
    {
        private readonly IBaseCommandRepository<UserReport> commandRepository;
        private readonly IBaseSharedRepository<UserReport> sharedRepository;
        private readonly IUserReportCommandRepository userReportCommandRepository;

        public UserReportCommandService(IBaseCommandRepository<UserReport> commandRepository,
                                        IBaseSharedRepository<UserReport> sharedRepository,
                                        IUserReportCommandRepository userReportCommandRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
            this.userReportCommandRepository = userReportCommandRepository;
        }

        public bool Create(UserReport entity)
        {
            entity.Id = DataGenerationHelper.GenerateRowId();
            entity.CreateDate = DateTime.Now;
            entity.LastModifyDate = DateTime.Now;
            return commandRepository.Create(entity);
        }

        public async Task<bool> DeleteAsync(UserReport entity)
        {
            entity = await sharedRepository.GetAsync(entity.Id);
            return commandRepository.Delete(entity);
        }

        public bool Update(UserReport entity)
        {
            entity.LastModifyDate = DateTime.Now;
            return commandRepository.Update(entity);
        }

        public async Task<bool> DeleteUserReportsByReportedUserIdAsync(Guid reportedUserId)
        {
            return await userReportCommandRepository.DeleteUserReportsAsync(reportedUserId);
        }
    }
}
