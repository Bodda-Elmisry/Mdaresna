using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Helpers;
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
    public class ClassRoomLanguageCommandService : IBaseCommandService<ClassRoomLanguage>, IClassRoomLanguageCommandService
    {
        private readonly IBaseCommandRepository<ClassRoomLanguage> commandRepository;
        private readonly IBaseSharedRepository<ClassRoomLanguage> sharedRepository;

        public ClassRoomLanguageCommandService(IBaseCommandRepository<ClassRoomLanguage> commandRepository,
            IBaseSharedRepository<ClassRoomLanguage> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(ClassRoomLanguage entity)
        {
            try
            {
                //entity.Id = DataGenerationHelper.GenerateRowId();
                entity.CreateDate = DateTime.Now;
                entity.LastModifyDate = DateTime.Now;
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(ClassRoomLanguage entity)
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

        public bool Update(ClassRoomLanguage entity)
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
