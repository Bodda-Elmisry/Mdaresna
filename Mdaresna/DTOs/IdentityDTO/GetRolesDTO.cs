namespace Mdaresna.DTOs.IdentityDTO
{
    public class GetRolesDTO
    {
        public int Type { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool? Activation { get; set; }
    }
}
