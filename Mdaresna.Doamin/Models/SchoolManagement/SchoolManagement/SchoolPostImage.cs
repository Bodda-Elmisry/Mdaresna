using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement
{
    public class SchoolPostImage
    {
        public Guid Id { get; set; }

        public Guid PostId { get; set; }

        [ForeignKey(nameof(PostId))]
        public SchoolPost Post { get; set; }

        public string ImageUrl { get; set; }
    }
}
