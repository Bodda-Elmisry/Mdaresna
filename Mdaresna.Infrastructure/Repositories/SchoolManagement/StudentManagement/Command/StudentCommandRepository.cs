using Mdaresna.Doamin.DTOs.StudentManagement;
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


        public async Task<StudentPayResultDTO> Pay(Student student)
        {
            try
            {
                student.IsPayed = true;
                student.Active = true;
                student.LastModifyDate = DateTime.Now;
                context.Students.Update(student);

                var school = await context.Schools.FirstOrDefaultAsync(s=> s.Id == student.SchoolId);

                if(school != null)
                {
                    school.AvailableCoins--;
                    context.Schools.Update(school);
                }
                await context.SaveChangesAsync();

                StudentPayResultDTO result = new StudentPayResultDTO
                {
                    Payed = true,
                    AvaialableCoins = school.AvailableCoins,
                };

                return result;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }




    }
}
