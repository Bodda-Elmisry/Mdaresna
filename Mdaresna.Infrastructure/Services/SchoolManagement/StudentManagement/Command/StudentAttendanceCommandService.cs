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
    public class StudentAttendanceCommandService : IBaseCommandService<StudentAttendance>, IStudentAttendanceCommandService
    {
        private readonly IBaseCommandRepository<StudentAttendance> commandRepository;
        private readonly IBaseSharedRepository<StudentAttendance> sharedRepository;

        public StudentAttendanceCommandService(IBaseCommandRepository<StudentAttendance> commandRepository,
            IBaseSharedRepository<StudentAttendance> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(StudentAttendance entity)
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

        public async Task<bool> DeleteAsync(StudentAttendance entity)
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

        public bool Update(StudentAttendance entity)
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
