using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Helpers;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Query
{
    public class SchoolPostQueryRepository : BaseQueryRepository<SchoolPost>, ISchoolPostQueryRepository
    {
        private readonly AppDbContext context;
        private readonly AppSettingDTO appSettings;

        public SchoolPostQueryRepository(AppDbContext context, 
                                         IOptions<AppSettingDTO> appSettings) : base(context)
        {
            this.context = context;
            this.appSettings = appSettings.Value;
        }

        public async Task<IEnumerable<PostResultDTO>> GetSchoolPostesWithImagesAsync(Guid schoolId, string searchText, int pageNumber)
        {
            int pagesize = this.appSettings.PageSize != null ? this.appSettings.PageSize.Value : 30;
            var query = context.SchoolPosts
                .Include(p => p.Poster)
                .Include(p => p.School)
                .Where(x => x.SchoolId == schoolId);

            query = !string.IsNullOrEmpty(searchText) ? query.Where(x => x.Content.Contains(searchText) || x.Poster.FirstName.Contains(searchText) || x.Poster.LastName.Contains(searchText) || (x.Poster.FirstName + x.Poster.LastName).Contains(searchText.Replace(" ", ""))) : query;

            
            var result = await query.Select(p => new PostResultDTO
            {
                Id = p.Id,
                Content = p.Content,
                PosterId = p.PosterId,
                PosterName = $"{p.Poster.FirstName} {p.Poster.LastName}",
                LastModifyDate = p.LastModifyDate ?? p.PostDate,
                Schoold = p.SchoolId,
                SchoolName = p.School.Name
            }).Skip((pageNumber - 1) * pagesize)
                                  .Take(pagesize).ToListAsync();

            foreach (var post in result)
            {
                post.Images = await getPostImages(post.Id);
            }

            return result.OrderByDescending(x => x.LastModifyDate);


        }

        public async Task<PostResultDTO> GetPostWithImagesAsync(Guid postId)
        {
            
            var post = await context.SchoolPosts
                .Include(p=> p.Poster)
                .Include(p => p.School)
                .FirstOrDefaultAsync(x => x.Id == postId);

            var result = new PostResultDTO
            {
                Id = post.Id,
                Content = post.Content,
                PosterId = post.PosterId,
                PosterName = $"{post.Poster.FirstName} {post.Poster.LastName}",
                Schoold = post.SchoolId,
                SchoolName = post.School.Name,
                LastModifyDate = post.LastModifyDate ?? post.PostDate
            };
            
            if (post != null)
            {
                result.Images = await getPostImages(postId);
            }

            return result;
        }

        private async Task<IEnumerable<string>> getPostImages(Guid postId)
        {
            return await context.SchoolPostImages
                .Where(x => x.PostId == postId)
                .Select(x => $"{SettingsHelper.GetAppUrl()}/{x.ImageUrl.Replace("\\", "/")}")
                .ToListAsync();
        }


    }
}
