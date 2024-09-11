using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
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
    public class ClassRoomActivityCommandService : IBaseCommandService<ClassRoomActivity>, IClassRoomActivityCommandService
    {
        private readonly IBaseCommandRepository<ClassRoomActivity> commandRepository;
        private readonly IBaseSharedRepository<ClassRoomActivity> sharedRepository;
        private readonly IBaseCommandBulkRepository<ClassRoomStudentActivity> studentActivityBulkRepo;

        public ClassRoomActivityCommandService(IBaseCommandRepository<ClassRoomActivity> commandRepository,
            IBaseSharedRepository<ClassRoomActivity> sharedRepository,
            IBaseCommandBulkRepository<ClassRoomStudentActivity> studentActivityBulkRepo)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
            this.studentActivityBulkRepo = studentActivityBulkRepo;
        }
        public bool Create(ClassRoomActivity entity)
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
        public async Task<bool> Create(ClassRoomActivity entity, IEnumerable<Guid> studentsList)
        {
            try
            {
                var result = false;
                entity.Id = DataGenerationHelper.GenerateRowId();
                entity.CreateDate = DateTime.Now;
                entity.LastModifyDate = DateTime.Now;
                var created = commandRepository.Create(entity);
                if (created && studentsList != null)
                {
                    var studentsObjects = studentsList.Select(s => new ClassRoomStudentActivity
                    {
                        StudentId = s,
                        ActivityId = entity.Id,
                        CreateDate = DateTime.Now,
                        LastModifyDate = DateTime.Now,
                        IsAttend = false,
                        Result = null
                    });

                    result = await studentActivityBulkRepo.CreateBulk(studentsObjects);
                }
                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(ClassRoomActivity entity)
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

        public bool Update(ClassRoomActivity entity)
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
