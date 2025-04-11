using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
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
    internal class ClassroomEmployeeCommandService : IBaseCommandService<ClassroomEmployee>, IClassroomEmployeeCommandService
    {
        private readonly IBaseCommandRepository<ClassroomEmployee> commandRepository;
        private readonly IBaseSharedRepository<ClassroomEmployee> sharedRepository;

        public ClassroomEmployeeCommandService(IBaseCommandRepository<ClassroomEmployee> commandRepository,
            IBaseSharedRepository<ClassroomEmployee> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }

        public bool Create(ClassroomEmployee entity)
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

        public async Task<bool> DeleteAsync(ClassroomEmployee entity)
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

        public bool Update(ClassroomEmployee entity)
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
