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
    public class ClassRoomStudentActivityCommandService : IBaseCommandService<ClassRoomStudentActivity>, IClassRoomStudentActivityCommandService
    {
        private readonly IBaseCommandRepository<ClassRoomStudentActivity> commandRepository;
        private readonly IBaseSharedRepository<ClassRoomStudentActivity> sharedRepository;

        public ClassRoomStudentActivityCommandService(IBaseCommandRepository<ClassRoomStudentActivity> commandRepository,
            IBaseSharedRepository<ClassRoomStudentActivity> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(ClassRoomStudentActivity entity)
        {
            try
            {
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(ClassRoomStudentActivity entity)
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

        public bool Update(ClassRoomStudentActivity entity)
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
