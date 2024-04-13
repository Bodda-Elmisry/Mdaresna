using Mdaresna.Doamin.Models.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.Identity
{
    public class ConfirmSMSKeyResultDTO
    {
        public bool Confirmed { get; set; }

        public User User { get; set; }

        public string MSG { get; set; }
    }
}
