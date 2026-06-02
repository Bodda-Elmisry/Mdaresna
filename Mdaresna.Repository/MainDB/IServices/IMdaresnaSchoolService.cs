using Mdaresna.Doamin.MainDB.Enums;
using Mdaresna.Repository.MainDB.DTOs;

namespace Mdaresna.Repository.MainDB.IServices;

public interface IMdaresnaSchoolService
{
    Task<bool> CreateSchoolAsync(Guid schoolId, string schoolName, List<CreateSchoolServiceDTO> schoolServicesList);

    Task<bool> ChangeSchoolActivation(Guid schoolId, bool isActive);
}
