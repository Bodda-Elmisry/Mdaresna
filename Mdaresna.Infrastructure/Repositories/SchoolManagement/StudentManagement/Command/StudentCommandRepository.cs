using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.StudentManagement.Command;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.StudentManagement.Command
{
    public class StudentCommandRepository : BaseCommandRepository<Student>, IStudentCommandRepository
    {
        private readonly AppDbContext context;

        public StudentCommandRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }


        public async Task<bool> Pay(Student student)
        {
            try
            {
                context.Students.Update(student);

                var school = await context.Schools.FirstOrDefaultAsync(s=> s.Id == student.SchoolId);

                if(school != null)
                {
                    school.AvailableCoins--;
                    context.Schools.Update(school);
                }
                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




    }
}
