using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.UserManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.UserManagement.Command
{
    public class SchoolUserCommandService : IBaseCommandService<SchoolUser>, ISchoolUserCommandService
    {
        private readonly IBaseCommandRepository<SchoolUser> commandRepository;
        private readonly IBaseSharedRepository<SchoolUser> sharedRepository;

        public SchoolUserCommandService(IBaseCommandRepository<SchoolUser> commandRepository,
            IBaseSharedRepository<SchoolUser> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(SchoolUser entity)
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

        public async Task<bool> DeleteAsync(SchoolUser entity)
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

        public bool Update(SchoolUser entity)
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
