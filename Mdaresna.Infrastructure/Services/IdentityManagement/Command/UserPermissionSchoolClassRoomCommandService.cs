using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Infrastructure.Repositories.Base;
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
        private readonly IBaseCommandBulkRepository<UserPermissionSchoolClassRoom> baseCommandBulkRepository;

        public UserPermissionSchoolClassRoomCommandService(IBaseCommandRepository<UserPermissionSchoolClassRoom> commandRepository,
            IBaseSharedRepository<UserPermissionSchoolClassRoom> sharedRepository,
            IBaseCommandBulkRepository<UserPermissionSchoolClassRoom> baseCommandBulkRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
            this.baseCommandBulkRepository = baseCommandBulkRepository;
        }
        public bool Create(UserPermissionSchoolClassRoom entity)
        {
            try
            {
                entity.CreateDate = DateTime.Now;
                entity.LastModifyDate = DateTime.Now;
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public async Task<bool> Create(IEnumerable<UserPermissionSchoolClassRoom> entities)
        {
            try
            {
                return await baseCommandBulkRepository.CreateBulk(entities);
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
                entity.LastModifyDate = DateTime.Now;
                return commandRepository.Update(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
