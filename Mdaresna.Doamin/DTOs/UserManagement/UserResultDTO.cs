using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.UserManagement
{
    public class UserResultDTO
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string FirstName { get; set; }
        public string? MiddelName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime? BirthDay { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public string? Email { get; set; }
    }
}
