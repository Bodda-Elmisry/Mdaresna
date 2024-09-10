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
    public class ClassRoomExamCommandService : IBaseCommandService<ClassRoomExam>, IClassRoomExamCommandService
    {
        private readonly IBaseCommandRepository<ClassRoomExam> commandRepository;
        private readonly IBaseSharedRepository<ClassRoomExam> sharedRepository;
        private readonly IBaseCommandBulkRepository<ClassRoomStudentExam> classRoomStudentExamBulkRepository;

        public ClassRoomExamCommandService(IBaseCommandRepository<ClassRoomExam> commandRepository,
            IBaseSharedRepository<ClassRoomExam> sharedRepository,
            IBaseCommandBulkRepository<ClassRoomStudentExam> classRoomStudentExamBulkRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
            this.classRoomStudentExamBulkRepository = classRoomStudentExamBulkRepository;
        }
        public bool Create(ClassRoomExam entity)
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

        public async Task<bool> Create(ClassRoomExam entity, IEnumerable<Guid> StudentsList)
        {
            try
            {
                var result = true;
                entity.Id = DataGenerationHelper.GenerateRowId();
                entity.CreateDate = DateTime.Now;
                entity.LastModifyDate = DateTime.Now;

                var created = commandRepository.Create(entity);

                if (created && StudentsList != null)
                {
                    var studentsExams = StudentsList.Select(e => new ClassRoomStudentExam
                    {
                        ExamId = entity.Id,
                        StudentId = e,
                        IsAttend = false,
                        TotalResult = null,
                        CreateDate = DateTime.Now,
                        LastModifyDate = DateTime.Now,
                    });

                    result = await classRoomStudentExamBulkRepository.CreateBulk(studentsExams);
                }

                return result;



            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> DeleteAsync(ClassRoomExam entity)
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

        public bool Update(ClassRoomExam entity)
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
