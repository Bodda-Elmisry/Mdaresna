using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command
{
    public interface IClassRoomAssignmentCommandService : IBaseCommandService<ClassRoomAssignment>
    {
        Task<bool> Create(ClassRoomAssignment entity, IEnumerable<Guid> studentsList);
    }
}
