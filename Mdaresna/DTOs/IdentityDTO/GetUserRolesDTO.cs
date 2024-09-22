namespace Mdaresna.DTOs.IdentityDTO
{
    public class GetUserRolesDTO 
    {
        public Guid UserId { get; set; }
        public Guid? SchoolId { get; set; }
    }
}
