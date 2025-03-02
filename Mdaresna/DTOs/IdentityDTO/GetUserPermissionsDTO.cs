namespace Mdaresna.DTOs.IdentityDTO
{
    public class GetUserPermissionsDTO
    {
        public Guid UserId { get; set; }
        public Guid? SchoolId { get; set; }
    }
}
