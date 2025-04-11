using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Command
{
    internal class SchoolEmployeeCommandService : IBaseCommandService<SchoolEmployee>, ISchoolEmployeeCommandService
    {
        private readonly IBaseCommandRepository<SchoolEmployee> commandRepository;
        private readonly IBaseSharedRepository<SchoolEmployee> sharedRepository;

        public SchoolEmployeeCommandService(IBaseCommandRepository<SchoolEmployee> commandRepository,
            IBaseSharedRepository<SchoolEmployee> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }

        public bool Create(SchoolEmployee entity)
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

        public async Task<bool> DeleteAsync(SchoolEmployee entity)
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

        public bool Update(SchoolEmployee entity)
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
