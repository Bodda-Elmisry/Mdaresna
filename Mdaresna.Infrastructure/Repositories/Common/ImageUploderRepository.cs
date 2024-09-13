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

        public async Task<bool> UploadImage(Guid UserId, string filePath, int type)
        {
            var result = false;
            switch(type)
            {
                case 1:
                    result = await UploadStudentImage(UserId, filePath);
                    break;
                case 2:
                    result = await UploadUserImage(UserId, filePath);
                    break;
                case 3:
                    result = await UploadSchoolPersonalImage(UserId, filePath);
                    break;
                case 5:
                    result = await UploadSchoolContactTypeIcon(UserId, filePath);
                    break;

            }

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

            //context.SaveChanges();
            await context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> UploadSchoolPersonalImage(Guid schoolId, string filePath)
        {
            var school = await context.Schools.FirstOrDefaultAsync(s=> s.Id == schoolId);

            if(school == null)
                return false;

            school.ImageUrl = filePath;

            await context.SaveChangesAsync();
            return true;
        }

        private async Task<bool> UploadSchoolContactTypeIcon(Guid TypeId, string filePath)
        {
            var school = await context.SchoolContactTypes.FirstOrDefaultAsync(s => s.Id == TypeId);

            if (school == null)
                return false;

            school.IconUrl = filePath;

            await context.SaveChangesAsync();
            return true;
        }




    }
}
