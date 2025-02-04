using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Command
{
    public class StudentCommandService : IBaseCommandService<Student>, IStudentCommandService
    {
        private readonly IBaseCommandRepository<Student> commandRepository;
        private readonly IBaseSharedRepository<Student> sharedRepository;
        private readonly IStudentCommandRepository studentCommandRepository;

        public StudentCommandService(IBaseCommandRepository<Student> commandRepository,
            IBaseSharedRepository<Student> sharedRepository,
            IStudentCommandRepository studentCommandRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
            this.studentCommandRepository = studentCommandRepository;
        }
        public bool Create(Student entity)
        {
            try
            {
                entity.Id = DataGenerationHelper.GenerateRowId();
                //entity.Code = GenerateStudentCode();
                entity.CreateDate = DateTime.Now;
                entity.LastModifyDate = DateTime.Now;
                return commandRepository.Create(entity);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(Student entity)
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

        public bool Update(Student entity)
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

        public async Task<StudentPayResultDTO> Pay(Student student)
        {
            try
            {
                return await studentCommandRepository.Pay(student);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
