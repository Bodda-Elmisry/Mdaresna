using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IRepositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Command;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Query;
using Mdaresna.Repository.IServices.Base;
using Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.StudentManagement.Command
{
    public class StudentAbsencePermitCommandService : IBaseCommandService<StudentAbsencePermit>, IStudentAbsencePermitCommandService
    {
        public const string PermitCreated = "Absence Permit Created";
        public const string PermitAlreadyExists = "Absence Permit Already Exists";
        public const string ParentNotLinked = "Student Parent Not Found";
        public const string StudentNotFound = "Student Not Found";

        private readonly IBaseCommandRepository<StudentAbsencePermit> commandRepository;
        private readonly IBaseSharedRepository<StudentAbsencePermit> sharedRepository;
        private readonly IStudentAbsencePermitQueryRepository studentAbsencePermitQueryRepository;
        private readonly IStudentParentQueryRepository studentParentQueryRepository;
        private readonly IStudentQueryRepository studentQueryRepository;

        public StudentAbsencePermitCommandService(
            IBaseCommandRepository<StudentAbsencePermit> commandRepository,
            IBaseSharedRepository<StudentAbsencePermit> sharedRepository,
            IStudentAbsencePermitQueryRepository studentAbsencePermitQueryRepository,
            IStudentParentQueryRepository studentParentQueryRepository,
            IStudentQueryRepository studentQueryRepository)
        {
            this.commandRepository = commandRepository;
            this.sharedRepository = sharedRepository;
            this.studentAbsencePermitQueryRepository = studentAbsencePermitQueryRepository;
            this.studentParentQueryRepository = studentParentQueryRepository;
            this.studentQueryRepository = studentQueryRepository;
        }

        public bool Create(StudentAbsencePermit entity)
        {
            entity.Id = DataGenerationHelper.GenerateRowId();
            entity.Date = entity.Date.Date;
            entity.CreateDate = DateTime.Now;
            entity.LastModifyDate = DateTime.Now;
            return commandRepository.Create(entity);
        }

        public async Task<bool> DeleteAsync(StudentAbsencePermit entity)
        {
            entity = await sharedRepository.GetAsync(entity.Id);
            entity.Deleted = true;
            entity.LastModifyDate = DateTime.Now;
            return commandRepository.Update(entity);
        }

        public bool Update(StudentAbsencePermit entity)
        {
            entity.LastModifyDate = DateTime.Now;
            return commandRepository.Update(entity);
        }

        public async Task<string> CreateAbsencePermitAsync(AddStudentAbsencePermitDTO permitDTO)
        {
            var studentParent = await studentParentQueryRepository.GetstudentParentByIdAsync(
                permitDTO.ParentId,
                permitDTO.StudentId);

            if (studentParent == null)
            {
                return ParentNotLinked;
            }

            var student = await studentQueryRepository.GetByIdAsync(permitDTO.StudentId);
            if (student == null)
            {
                return StudentNotFound;
            }

            var permitDate = permitDTO.Date.Date;
            var existingPermit = await studentAbsencePermitQueryRepository.GetActivePermitAsync(
                permitDTO.StudentId,
                permitDate);

            if (existingPermit != null)
            {
                return PermitAlreadyExists;
            }

            var permit = new StudentAbsencePermit
            {
                StudentId = permitDTO.StudentId,
                ParentId = permitDTO.ParentId,
                ClassRoomId = student.ClassRoomId,
                Date = permitDate,
                Reason = string.IsNullOrWhiteSpace(permitDTO.Reason)
                    ? null
                    : permitDTO.Reason.Trim()
            };

            return Create(permit) ? PermitCreated : "Error";
        }

        public async Task<bool> SoftDeleteAbsencePermitAsync(Guid permitId, Guid parentId)
        {
            var permit = await sharedRepository.GetAsync(permitId);
            if (permit == null || permit.Deleted || permit.ParentId != parentId)
            {
                return false;
            }

            permit.Deleted = true;
            permit.LastModifyDate = DateTime.Now;
            return commandRepository.Update(permit);
        }
    }
}
