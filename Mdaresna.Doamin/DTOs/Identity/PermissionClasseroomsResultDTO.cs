using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.Identity
{
    public class PermissionClasseroomsResultDTO
    {
        public string PermissionKey { get; set; }
        public IEnumerable<Guid> Classrooms { get; set; }
    }
}
