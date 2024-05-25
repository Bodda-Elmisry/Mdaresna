using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Command
{
    public class ClassRoomStudentAssignmentCommandService : IBaseCommandService<ClassRoomStudentAssignment>, IClassRoomStudentAssignmentCommandService
    {
        private readonly IBaseCommandRepository<ClassRoomStudentAssignment> commandRepository;
        private readonly IBaseSharedRepository<ClassRoomStudentAssignment> sharedRepository;

        public ClassRoomStudentAssignmentCommandService(IBaseCommandRepository<ClassRoomStudentAssignment> commandRepository,
            IBaseSharedRepository<ClassRoomStudentAssignment> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(ClassRoomStudentAssignment entity)
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

        public async Task<bool> DeleteAsync(ClassRoomStudentAssignment entity)
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

        public bool Update(ClassRoomStudentAssignment entity)
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
