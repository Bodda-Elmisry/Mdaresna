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
    public class StudentExamRateCommandService : IBaseCommandService<StudentExamRate>, IStudentExamRateCommandService
    {
        private readonly IBaseCommandRepository<StudentExamRate> commandRepository;
        private readonly IBaseSharedRepository<StudentExamRate> sharedRepository;

        public StudentExamRateCommandService(IBaseCommandRepository<StudentExamRate> commandRepository,
            IBaseSharedRepository<StudentExamRate> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(StudentExamRate entity)
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

        public async Task<bool> DeleteAsync(StudentExamRate entity)
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

        public bool Update(StudentExamRate entity)
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
