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
    public class SchoolCourseCommandService : IBaseCommandService<SchoolCourse>, ISchoolCourseCommandService
    {
        private readonly IBaseCommandRepository<SchoolCourse> commandRepository;
        private readonly IBaseSharedRepository<SchoolCourse> sharedRepository;

        public SchoolCourseCommandService(IBaseCommandRepository<SchoolCourse> commandRepository,
            IBaseSharedRepository<SchoolCourse> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(SchoolCourse entity)
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

        public async Task<bool> DeleteAsync(SchoolCourse entity)
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

        public bool Update(SchoolCourse entity)
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
