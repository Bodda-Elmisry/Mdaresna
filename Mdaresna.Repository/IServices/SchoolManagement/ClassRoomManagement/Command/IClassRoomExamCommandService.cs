using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SchoolManagement.ClassRoomManagement.Command
{
    public interface IClassRoomExamCommandService : IBaseCommandService<ClassRoomExam>
    {
        Task<bool> Create(ClassRoomExam entity, IEnumerable<Guid> StudentsList);
    }
}
