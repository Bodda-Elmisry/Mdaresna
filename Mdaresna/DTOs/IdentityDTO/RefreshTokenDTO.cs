using System;

namespace Mdaresna.DTOs.IdentityDTO
{
    public class RefreshTokenDTO
    {
        public string RefreshToken { get; set; }
        public Guid? SchoolId { get; set; }
    }
}
