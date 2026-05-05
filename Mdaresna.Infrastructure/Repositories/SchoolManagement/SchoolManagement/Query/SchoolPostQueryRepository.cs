using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Helpers;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Query;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Mdaresna.Infrastructure.Repositories.SchoolManagement.SchoolManagement.Query
{
    public class SchoolPostQueryRepository : BaseQueryRepository<SchoolPost>, ISchoolPostQueryRepository
    {
        private readonly AppDbContext context;
        private readonly AppSettingDTO appSettings;

        public SchoolPostQueryRepository(
            AppDbContext context,
            IOptions<AppSettingDTO> appSettings) : base(context)
        {
            this.context = context;
            this.appSettings = appSettings.Value;
        }

        public async Task<IEnumerable<PostResultDTO>> GetSchoolPostesWithImagesAsync(
            Guid schoolId,
            Guid? viewerUserId,
            string searchText,
            int pageNumber)
        {
            int pageSize = appSettings.PageSize != null ? appSettings.PageSize.Value : 30;
            var query = context.SchoolPosts
                .Include(p => p.Poster)
                .Include(p => p.School)
                .Where(x =>
                    x.SchoolId == schoolId &&
                    x.Deleted == false &&
                    x.ModerationStatus == SchoolPostModerationStatusEnum.Approved);

            if (viewerUserId.HasValue && viewerUserId.Value != Guid.Empty)
            {
                query = query.Where(x => !context.UserBlocks.Any(b =>
                    b.Deleted == false &&
                    b.BlockerUserId == viewerUserId.Value &&
                    b.BlockedUserId == x.PosterId));
            }

            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(x =>
                    x.Content.Contains(searchText) ||
                    x.Poster.FirstName.Contains(searchText) ||
                    x.Poster.LastName.Contains(searchText) ||
                    (x.Poster.FirstName + x.Poster.LastName).Contains(searchText.Replace(" ", "")));
            }

            var result = await query
                .OrderByDescending(x => x.LastModifyDate ?? x.PostDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PostResultDTO
                {
                    Id = p.Id,
                    Content = p.Content,
                    PosterId = p.PosterId,
                    PosterName = $"{p.Poster.FirstName} {p.Poster.LastName}",
                    LastModifyDate = p.LastModifyDate ?? p.PostDate,
                    Schoold = p.SchoolId,
                    SchoolName = p.School.Name,
                    ModerationStatus = p.ModerationStatus.ToString(),
                    ModerationReason = p.ModerationReason
                })
                .ToListAsync();

            foreach (var post in result)
            {
                post.Images = await GetPostImages(post.Id);
            }

            return result;
        }

        public async Task<IEnumerable<SchoolPostReportsCountResultDTO>> GetPostsWithReportsCountAsync(
            Guid? schoolId,
            string? schoolName,
            int? minReportsCount,
            int? maxReportsCount,
            int pageNumber)
        {
            int pageSize = appSettings.PageSize != null ? appSettings.PageSize.Value : 30;
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;
            var normalizedSchoolName = string.IsNullOrWhiteSpace(schoolName) ? null : schoolName.Trim();

            var postsQuery = context.SchoolPosts
                .Include(p => p.Poster)
                .Include(p => p.School)
                .Where(x => x.Deleted == false);

            if (schoolId.HasValue && schoolId.Value != Guid.Empty)
            {
                postsQuery = postsQuery.Where(x => x.SchoolId == schoolId.Value);
            }

            if (!string.IsNullOrEmpty(normalizedSchoolName))
            {
                postsQuery = postsQuery.Where(x => x.School.Name.Contains(normalizedSchoolName));
            }

            var postsWithReports = postsQuery
                .GroupJoin(
                    context.SchoolPostReports.Where(r => r.Deleted == false),
                    post => post.Id,
                    report => report.PostId,
                    (post, reports) => new
                    {
                        Post = post,
                        ReportsCount = reports.Count()
                    })
                .Where(x =>
                    x.ReportsCount > 0 ||
                    x.Post.ModerationStatus != SchoolPostModerationStatusEnum.Approved);

            if (minReportsCount.HasValue)
            {
                postsWithReports = postsWithReports.Where(x => x.ReportsCount >= minReportsCount.Value);
            }

            if (maxReportsCount.HasValue)
            {
                postsWithReports = postsWithReports.Where(x => x.ReportsCount <= maxReportsCount.Value);
            }

            var result = await postsWithReports
                .OrderBy(x => x.Post.ModerationStatus == SchoolPostModerationStatusEnum.PendingReview ? 0 : 1)
                .ThenByDescending(x => x.Post.LastModifyDate ?? x.Post.PostDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new SchoolPostReportsCountResultDTO
                {
                    PostId = x.Post.Id,
                    Content = x.Post.Content,
                    PosterId = x.Post.PosterId,
                    PosterName = $"{x.Post.Poster.FirstName} {x.Post.Poster.LastName}",
                    SchoolId = x.Post.SchoolId,
                    SchoolName = x.Post.School.Name,
                    ReportsCount = x.ReportsCount,
                    LastModifyDate = x.Post.LastModifyDate ?? x.Post.PostDate,
                    ModerationStatus = x.Post.ModerationStatus.ToString(),
                    ModerationReason = x.Post.ModerationReason
                })
                .ToListAsync();

            return result;
        }

        public async Task<PostResultDTO> GetPostWithImagesAsync(Guid postId)
        {
            var post = await context.SchoolPosts
                .Include(p => p.Poster)
                .Include(p => p.School)
                .FirstOrDefaultAsync(x =>
                    x.Id == postId &&
                    x.Deleted == false &&
                    x.ModerationStatus == SchoolPostModerationStatusEnum.Approved);

            if (post == null)
            {
                return null;
            }

            var result = new PostResultDTO
            {
                Id = post.Id,
                Content = post.Content,
                PosterId = post.PosterId,
                PosterName = $"{post.Poster.FirstName} {post.Poster.LastName}",
                Schoold = post.SchoolId,
                SchoolName = post.School.Name,
                LastModifyDate = post.LastModifyDate ?? post.PostDate,
                ModerationStatus = post.ModerationStatus.ToString(),
                ModerationReason = post.ModerationReason
            };

            result.Images = await GetPostImages(postId);
            return result;
        }

        private async Task<IEnumerable<string>> GetPostImages(Guid postId)
        {
            return await context.SchoolPostImages
                .Where(x => x.PostId == postId)
                .Select(x => $"{SettingsHelper.GetAppUrl()}/{x.ImageUrl.Replace("\\", "/")}")
                .ToListAsync();
        }
    }
}
