using System;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Query
{
    public interface ISchoolAccessValidator
    {
        Task<bool> CanAccessSchoolAsync(Guid userId, Guid schoolId);
        Task<bool> CanAccessStudentAsync(Guid userId, Guid studentId);
        Task<bool> CanAccessClassRoomAsync(Guid userId, Guid classRoomId);
        void RemoveSchoolAccessCache(Guid userId, Guid schoolId);
    }
}
