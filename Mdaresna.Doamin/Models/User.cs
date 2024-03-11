using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models
{
    public class User
    {
        public Guid Id { get; set; }
        
        [MaxLength(200)]
        public string? UserName { get; set; }
        
        [MaxLength(200)]
        public string FirstName { get; set; }
        
        [MaxLength(200)]
        public string? MiddelName { get; set; }
        
        [MaxLength(200)]
        public string LastName { get; set; }
        
        [MaxLength(200)]
        public string PhoneNumber { get; set; }

        [MaxLength(800)]
        public string Password { get; set; }
        
        public string? ImageUrl { get; set; }
        
        public DateTime? BirthDay { get; set; }

        [MaxLength(500)]
        public string? Address { get; set; }
        
        [MaxLength(300)]
        public string? City { get; set; }
        
        [MaxLength(300)]
        public string? Region { get; set; }
        
        [MaxLength(300)]
        public string? Contry { get; set; }

        public int UserType { get; set; }

    }
}
