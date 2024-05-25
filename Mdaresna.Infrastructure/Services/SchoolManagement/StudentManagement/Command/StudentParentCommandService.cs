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
    public class StudentParentCommandService : IBaseCommandService<StudentParent>, IStudentParentCommandService
    {
        private readonly IBaseCommandRepository<StudentParent> commandRepository;
        private readonly IBaseSharedRepository<StudentParent> sharedRepository;

        public StudentParentCommandService(IBaseCommandRepository<StudentParent> commandRepository,
            IBaseSharedRepository<StudentParent> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(StudentParent entity)
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

        public async Task<bool> DeleteAsync(StudentParent entity)
        {
            try
            {
                entity.LastModifyDate = DateTime.Now;
                return commandRepository.Delete(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Update(StudentParent entity)
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
