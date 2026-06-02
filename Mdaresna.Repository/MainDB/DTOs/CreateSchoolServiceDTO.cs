using Mdaresna.Doamin.MainDB.Enums;

namespace Mdaresna.Repository.MainDB.DTOs;

public record CreateSchoolServiceDTO(Guid ServiceId, DBTypeEnum DBType);
