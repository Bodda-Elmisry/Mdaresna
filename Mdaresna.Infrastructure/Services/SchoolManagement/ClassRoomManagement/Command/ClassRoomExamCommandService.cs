using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.ClassRoomManagement.Command
{
    public class ClassRoomExamCommandService : IBaseCommandService<ClassRoomExam>, IClassRoomExamCommandService
    {
        private readonly IBaseCommandRepository<ClassRoomExam> commandRepository;
        private readonly IBaseSharedRepository<ClassRoomExam> sharedRepository;

        public ClassRoomExamCommandService(IBaseCommandRepository<ClassRoomExam> commandRepository,
            IBaseSharedRepository<ClassRoomExam> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(ClassRoomExam entity)
        {
            try
            {
                entity.Id = DataGenerationHelper.GenerateRowId();
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(ClassRoomExam entity)
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

        public bool Update(ClassRoomExam entity)
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
