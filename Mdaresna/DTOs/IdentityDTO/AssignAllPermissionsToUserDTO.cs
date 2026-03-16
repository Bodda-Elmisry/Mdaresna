namespace Mdaresna.DTOs.IdentityDTO
{
    public class AssignAllPermissionsToUserDTO
    {
        public Guid UserId { get; set; }
        public bool IsSchoolUser { get; set; }
        public Guid? SchoolId { get; set; }
    }
}
