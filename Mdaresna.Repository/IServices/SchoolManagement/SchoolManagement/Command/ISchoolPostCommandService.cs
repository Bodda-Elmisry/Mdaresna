using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command
{
    public interface ISchoolPostCommandService : IBaseCommandService<SchoolPost>
    {
        Task<bool> CreateAsync(SchoolPost entity, IEnumerable<string>? images);
    }
}
