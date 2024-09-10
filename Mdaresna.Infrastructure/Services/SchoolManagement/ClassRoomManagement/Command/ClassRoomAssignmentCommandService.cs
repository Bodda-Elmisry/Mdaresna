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
    public class ClassRoomAssignmentCommandService : IBaseCommandService<ClassRoomAssignment>, IClassRoomAssignmentCommandService
    {
        private readonly IBaseCommandRepository<ClassRoomAssignment> commandRepository;
        private readonly IBaseSharedRepository<ClassRoomAssignment> sharedRepository;
        private readonly IBaseCommandBulkRepository<ClassRoomStudentAssignment> studentAssignmentBulkRepo;

        public ClassRoomAssignmentCommandService(IBaseCommandRepository<ClassRoomAssignment> commandRepository,
            IBaseSharedRepository<ClassRoomAssignment> sharedRepository,
            IBaseCommandBulkRepository<ClassRoomStudentAssignment> studentAssignmentBulkRepo)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
            this.studentAssignmentBulkRepo = studentAssignmentBulkRepo;
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

        public async Task<bool> Create(ClassRoomAssignment entity, IEnumerable<Guid> studentsList)
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
                    var studentsObjects = studentsList.Select(s => new ClassRoomStudentAssignment
                    {
                        StudentId = s,
                        AssignmentId = entity.Id,
                        CreateDate = DateTime.Now,
                        LastModifyDate = DateTime.Now,
                        IsDelivered = null,
                        DeliveredDate = null,
                        Result = 0
                    });

                    result = await studentAssignmentBulkRepo.CreateBulk(studentsObjects);
                }

                return result;
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
