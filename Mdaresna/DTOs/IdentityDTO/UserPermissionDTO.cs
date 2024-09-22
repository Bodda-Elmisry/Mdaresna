using Mdaresna.DTOs.Common;

namespace Mdaresna.DTOs.IdentityDTO
{
    public class UserPermissionDTO : UserIdSchoolIdDTO
    {
        public Guid PermissionId { get; set; }

    }
}
