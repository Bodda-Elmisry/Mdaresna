using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.IRepositories.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.Common
{
    public class ImageUploderRepository : IImageUploderRepository
    {
        private readonly AppDbContext context;

        public ImageUploderRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<bool> UploadImage(Guid UserId, string filePath, bool isStudent)
        {
            var result = false;
            if(isStudent)
                result = await UploadStudentImage(UserId, filePath);
            else
                result = await UploadUserImage(UserId, filePath);

            return result;
        }

        private async Task<bool> UploadStudentImage(Guid UserId, string filePath)
        {
            var student = await context.Students.FirstOrDefaultAsync(s=> s.Id  == UserId);
            if (student == null)
                return false;

            student.ImageUrl = filePath;

            context.SaveChanges();
            await context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> UploadUserImage(Guid UserId, string filePath)
        {
            var users = await context.Users.FirstOrDefaultAsync(s => s.Id == UserId);
            if (users == null)
                return false;

            users.ImageUrl = filePath;

            context.SaveChanges();
            await context.SaveChangesAsync();
            return true;
        }




    }
}
