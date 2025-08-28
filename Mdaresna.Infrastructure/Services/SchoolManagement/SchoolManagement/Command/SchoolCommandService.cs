using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Command;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Command
{
    public class SchoolCommandService : IBaseCommandService<School>, ISchoolCommandService
    {
        private readonly IBaseCommandRepository<School> commandRepository;
        private readonly IBaseSharedRepository<School> sharedRepository;
        private readonly ISchoolCommandRepository schoolCommandRepository;

        public SchoolCommandService(IBaseCommandRepository<School> commandRepository,
            IBaseSharedRepository<School> sharedRepository,
            ISchoolCommandRepository schoolCommandRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
            this.schoolCommandRepository = schoolCommandRepository;
        }
        public bool Create(School entity)
        {
            try
            {
                entity.Id = DataGenerationHelper.GenerateRowId(); 
                entity.CreateDate = DateTime.Now;
                entity.LastModifyDate = DateTime.Now;
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(School entity)
        {
            try
            {
                entity = await sharedRepository.GetAsync(entity.Id);
                return commandRepository.Delete(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteSchoolImageByImageNameAsync(string imageName)
        {
            return await schoolCommandRepository.DeleteSchoolImageByImageNameAsync(imageName);
        }

        public bool Update(School entity)
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
