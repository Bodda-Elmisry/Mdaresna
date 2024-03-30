using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Infrastructure.Hilpers;
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
    public class ClassRoomActivityCommandService : IBaseCommandService<ClassRoomActivity>, IClassRoomActivityCommandService
    {
        private readonly IBaseCommandRepository<ClassRoomActivity> commandRepository;
        private readonly IBaseSharedRepository<ClassRoomActivity> sharedRepository;

        public ClassRoomActivityCommandService(IBaseCommandRepository<ClassRoomActivity> commandRepository,
            IBaseSharedRepository<ClassRoomActivity> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(ClassRoomActivity entity)
        {
            try
            {
                entity.Id = DataGenerationHilper.GenerateRowId();
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(ClassRoomActivity entity)
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

        public bool Update(ClassRoomActivity entity)
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
