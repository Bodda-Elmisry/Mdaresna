using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.SchoolManagement
{
    public class PostResultDTO
    {
        public Guid Id { get; set; }
        public DateTime LastModifyDate { get; set; }
        public string Content { get; set; }
        public Guid PosterId { get; set; }
        public string PosterName { get; set; }
        public Guid Schoold { get; set; }
        public string SchoolName { get; set; }
        public IEnumerable<string> Images { get; set; }


    }
}
