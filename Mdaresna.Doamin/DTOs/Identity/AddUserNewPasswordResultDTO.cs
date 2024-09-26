using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.Identity
{
    public class AddUserNewPasswordResultDTO
    {
        public bool PasswordChanged { get; set; }
        public string MSG { get; set; }
    }
}
