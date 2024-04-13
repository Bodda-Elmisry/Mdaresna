using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Command
{
    public class SchoolTeacherCourseCommandService : IBaseCommandService<SchoolTeacherCourse>, ISchoolTeacherCourseCommandService
    {
        private readonly IBaseCommandRepository<SchoolTeacherCourse> commandRepository;
        private readonly IBaseSharedRepository<SchoolTeacherCourse> sharedRepository;

        public SchoolTeacherCourseCommandService(IBaseCommandRepository<SchoolTeacherCourse> commandRepository,
            IBaseSharedRepository<SchoolTeacherCourse> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(SchoolTeacherCourse entity)
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

        public async Task<bool> DeleteAsync(SchoolTeacherCourse entity)
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

        public bool Update(SchoolTeacherCourse entity)
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
