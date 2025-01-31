using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SchoolManagement.StudentManagement.Command
{
    public interface IStudentCommandService : IBaseCommandService<Student>
    {
        Task<bool> Pay(Student student);
    }
}
