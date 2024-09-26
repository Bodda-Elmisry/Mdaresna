using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.Identity
{
    public class ForgetPasseordResultDTO
    {
        public bool ConfermationKeySent { get; set; }
        public Guid? UserId { get; set; }
        public string MSG { get; set; }
    }
}
