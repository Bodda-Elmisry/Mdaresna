using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Infrastructure.Hilpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.IdentityManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.IdentityManagement.Command
{
    public class UserPermissionSchoolClassRoomCommandService : IBaseCommandService<UserPermissionSchoolClassRoom>, IUserPermissionSchoolClassRoomCommandService
    {
        private readonly IBaseCommandRepository<UserPermissionSchoolClassRoom> commandRepository;
        private readonly IBaseSharedRepository<UserPermissionSchoolClassRoom> sharedRepository;

        public UserPermissionSchoolClassRoomCommandService(IBaseCommandRepository<UserPermissionSchoolClassRoom> commandRepository,
            IBaseSharedRepository<UserPermissionSchoolClassRoom> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(UserPermissionSchoolClassRoom entity)
        {
            try
            {
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(UserPermissionSchoolClassRoom entity)
        {
            try
            {
                return commandRepository.Delete(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Update(UserPermissionSchoolClassRoom entity)
        {
            try
            {
                return commandRepository.Update(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
