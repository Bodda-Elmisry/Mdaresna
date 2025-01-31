using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Command
{
    public interface IStudentCommandRepository : IBaseCommandRepository<Student>
    {
        Task<bool> Pay(Student student);
    }
}
