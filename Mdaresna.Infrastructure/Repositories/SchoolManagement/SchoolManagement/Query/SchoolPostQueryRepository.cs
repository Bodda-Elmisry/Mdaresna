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
                .Where(x => x.SchoolId == schoolId && x.Deleted == false);

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

        public async Task<IEnumerable<SchoolPostReportsCountResultDTO>> GetPostsWithReportsCountAsync(string? schoolName, int? minReportsCount, int? maxReportsCount, int pageNumber)
        {
            int pagesize = this.appSettings.PageSize != null ? this.appSettings.PageSize.Value : 30;
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            var normalizedSchoolName = string.IsNullOrWhiteSpace(schoolName) ? null : schoolName.Trim();

            var postsQuery = context.SchoolPosts
                                    .Include(p => p.Poster)
                                    .Include(p => p.School)
                                    .Where(x => x.Deleted == false);

            if (!string.IsNullOrEmpty(normalizedSchoolName))
            {
                postsQuery = postsQuery.Where(x => x.School.Name.Contains(normalizedSchoolName));
            }

            var postsWithReports = postsQuery
                                    .GroupJoin(context.SchoolPostReports.Where(r => r.Deleted == false),
                                               post => post.Id,
                                               report => report.PostId,
                                               (post, reports) => new
                                               {
                                                   Post = post,
                                                   ReportsCount = reports.Count()
                                               });

            if (minReportsCount.HasValue)
            {
                postsWithReports = postsWithReports.Where(x => x.ReportsCount >= minReportsCount.Value);
            }

            if (maxReportsCount.HasValue)
            {
                postsWithReports = postsWithReports.Where(x => x.ReportsCount <= maxReportsCount.Value);
            }

            var result = await postsWithReports
                .OrderByDescending(x => x.Post.LastModifyDate ?? x.Post.PostDate)
                .Skip((pageNumber - 1) * pagesize)
                .Take(pagesize)
                .Select(x => new SchoolPostReportsCountResultDTO
                {
                    PostId = x.Post.Id,
                    Content = x.Post.Content,
                    PosterId = x.Post.PosterId,
                    PosterName = $"{x.Post.Poster.FirstName} {x.Post.Poster.LastName}",
                    SchoolId = x.Post.SchoolId,
                    SchoolName = x.Post.School.Name,
                    ReportsCount = x.ReportsCount,
                    LastModifyDate = x.Post.LastModifyDate ?? x.Post.PostDate
                }).ToListAsync();

            return result;
        }

        public async Task<PostResultDTO> GetPostWithImagesAsync(Guid postId)
        {
            
            var post = await context.SchoolPosts
                .Include(p=> p.Poster)
                .Include(p => p.School)
                .FirstOrDefaultAsync(x => x.Id == postId && x.Deleted == false);

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
