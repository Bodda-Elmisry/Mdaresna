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
    public class ClassRoomTeacherCourseCommandService : IBaseCommandService<ClassRoomTeacherCourse>, IClassRoomTeacherCourseCommandService
    {
        private readonly IBaseCommandRepository<ClassRoomTeacherCourse> commandRepository;
        private readonly IBaseSharedRepository<ClassRoomTeacherCourse> sharedRepository;

        public ClassRoomTeacherCourseCommandService(IBaseCommandRepository<ClassRoomTeacherCourse> commandRepository,
            IBaseSharedRepository<ClassRoomTeacherCourse> sharedRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
        }
        public bool Create(ClassRoomTeacherCourse entity)
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

        public async Task<bool> DeleteAsync(ClassRoomTeacherCourse entity)
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

        public bool Update(ClassRoomTeacherCourse entity)
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
