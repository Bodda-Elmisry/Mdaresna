using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Command;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Command
{
    public class SchoolCommandRepository : BaseCommandRepository<School>, ISchoolCommandRepository
    {
        private readonly AppDbContext context;

        public SchoolCommandRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public async Task<bool> DeleteSchoolImageByImageNameAsync(string imageName)
        {
            var namePartes = imageName.Split('/');
            var name = namePartes[namePartes.Length - 1];

            //var schoolImage = await context.SchoolImages.FirstOrDefaultAsync(s => s.ImagePath.EndsWith(name));

            var schoolImageQuery = context.SchoolImages.Where(s=> s.ImagePath.EndsWith(name));
            //Console.WriteLine(schoolImageQuery.ToQueryString());
            var row = await schoolImageQuery.FirstOrDefaultAsync();



            if (row == null) return false;

            Console.WriteLine($"Rows id = {row.Id}");

            context.Remove(row);

            return await context.SaveChangesAsync() > 0;
            


        }


    }
}
