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
    public class ClassRoomAssignmentCommandService : IBaseCommandService<ClassRoomAssignment>, IClassRoomAssignmentCommandService
    {
        private readonly IBaseCommandRepository<ClassRoomAssignment> commandRepository;
        private readonly IBaseSharedRepository<ClassRoomAssignment> sharedRepository;

        public ClassRoomAssignmentCommandService(IBaseCommandRepository<ClassRoomAssignment> commandRepository,
            IBaseSharedRepository<ClassRoomAssignment> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(ClassRoomAssignment entity)
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

        public async Task<bool> DeleteAsync(ClassRoomAssignment entity)
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

        public bool Update(ClassRoomAssignment entity)
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
