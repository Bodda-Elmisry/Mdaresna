using System.ComponentModel.DataAnnotations;

namespace Mdaresna.DTOs.IdentityDTO
{
    public class SaveUserMainInfoDTO
    {
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string? UserName { get; set; }

        [MaxLength(200)]
        public string FirstName { get; set; }

        [MaxLength(200)]
        public string LastName { get; set; }

        [MaxLength(800)]
        public string Password { get; set; }

        public string? ImageUrl { get; set; }

        //public string EncriptionKey { get; set; }



    }
}
