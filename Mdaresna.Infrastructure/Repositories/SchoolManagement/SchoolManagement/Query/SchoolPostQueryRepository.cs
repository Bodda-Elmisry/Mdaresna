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
            bool includeSchoolMembers,
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
                    x.ModerationStatus == SchoolPostModerationStatusEnum.Approved &&
                    (x.Visibility == SchoolPostVisibilityEnum.Public ||
                     (includeSchoolMembers && x.Visibility == SchoolPostVisibilityEnum.SchoolMembers)));

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
                    ModerationReason = p.ModerationReason,
                    Visibility = p.Visibility.ToString(),
                    ReactionsCount = context.SchoolPostReactions.Count(reaction =>
                        reaction.PostId == p.Id &&
                        !reaction.Deleted &&
                        !reaction.User.Deleted),
                    CurrentUserReaction = viewerUserId.HasValue
                        ? context.SchoolPostReactions
                            .Where(reaction =>
                                reaction.PostId == p.Id &&
                                reaction.UserId == viewerUserId.Value &&
                                !reaction.Deleted)
                            .Select(reaction =>
                                reaction.ReactionType == SchoolPostReactionTypeEnum.Like
                                    ? "Like"
                                    : null)
                            .FirstOrDefault()
                        : null
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

            var reportsCountQuery = context.SchoolPostReports
                .AsNoTracking()
                .Where(report => report.Deleted == false)
                .GroupBy(report => report.PostId)
                .Select(group => new
                {
                    PostId = group.Key,
                    ReportsCount = group.Count()
                });

            var postsWithReports =
                from post in context.SchoolPosts.AsNoTracking()
                join reportsCount in reportsCountQuery
                    on post.Id equals reportsCount.PostId into reportsCounts
                from reportsCount in reportsCounts.DefaultIfEmpty()
                join poster in context.Users.AsNoTracking()
                    on post.PosterId equals poster.Id
                join school in context.Schools.AsNoTracking()
                    on post.SchoolId equals school.Id
                where post.Deleted == false
                select new
                {
                    PostId = post.Id,
                    post.Content,
                    post.PosterId,
                    post.SchoolId,
                    SchoolName = school.Name,
                    post.ModerationStatus,
                    post.ModerationReason,
                    post.Visibility,
                    ReactionsCount = context.SchoolPostReactions.Count(reaction =>
                        reaction.PostId == post.Id &&
                        !reaction.Deleted &&
                        !reaction.User.Deleted),
                    LastModifyDate = post.LastModifyDate ?? post.PostDate,
                    ReportsCount = (int?)reportsCount.ReportsCount ?? 0,
                    PosterFirstName = poster.FirstName,
                    PosterLastName = poster.LastName
                };

            if (schoolId.HasValue && schoolId.Value != Guid.Empty)
            {
                postsWithReports = postsWithReports.Where(x => x.SchoolId == schoolId.Value);
            }

            if (!string.IsNullOrEmpty(normalizedSchoolName))
            {
                postsWithReports = postsWithReports.Where(x => x.SchoolName.Contains(normalizedSchoolName));
            }

            postsWithReports = postsWithReports.Where(x =>
                x.ReportsCount > 0 ||
                x.ModerationStatus != SchoolPostModerationStatusEnum.Approved);

            if (minReportsCount.HasValue)
            {
                postsWithReports = postsWithReports.Where(x => x.ReportsCount >= minReportsCount.Value);
            }

            if (maxReportsCount.HasValue)
            {
                postsWithReports = postsWithReports.Where(x => x.ReportsCount <= maxReportsCount.Value);
            }

            var rows = await postsWithReports
                .OrderBy(x => x.ModerationStatus)
                .ThenBy(x => x.LastModifyDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var result = rows
                .Select(x => new SchoolPostReportsCountResultDTO
                {
                    PostId = x.PostId,
                    Content = x.Content,
                    PosterId = x.PosterId,
                    PosterName = $"{x.PosterFirstName} {x.PosterLastName}",
                    SchoolId = x.SchoolId,
                    SchoolName = x.SchoolName,
                    ReportsCount = x.ReportsCount,
                    LastModifyDate = x.LastModifyDate,
                    ModerationStatus = x.ModerationStatus.ToString(),
                    ModerationReason = x.ModerationReason,
                    Visibility = x.Visibility.ToString(),
                    ReactionsCount = x.ReactionsCount
                })
                .ToList();

            return result;
        }

        public async Task<PostResultDTO> GetPostWithImagesAsync(
            Guid postId,
            Guid? viewerUserId,
            bool includeSchoolMembers)
        {
            var post = await context.SchoolPosts
                .Include(p => p.Poster)
                .Include(p => p.School)
                .FirstOrDefaultAsync(x =>
                    x.Id == postId &&
                    x.Deleted == false &&
                    x.ModerationStatus == SchoolPostModerationStatusEnum.Approved &&
                    (x.Visibility == SchoolPostVisibilityEnum.Public ||
                     (includeSchoolMembers && x.Visibility == SchoolPostVisibilityEnum.SchoolMembers)) &&
                    (!viewerUserId.HasValue || !context.UserBlocks.Any(block =>
                        !block.Deleted &&
                        block.BlockerUserId == viewerUserId.Value &&
                        block.BlockedUserId == x.PosterId)));

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
                ModerationReason = post.ModerationReason,
                Visibility = post.Visibility.ToString(),
                ReactionsCount = await context.SchoolPostReactions.CountAsync(reaction =>
                    reaction.PostId == post.Id &&
                    !reaction.Deleted &&
                    !reaction.User.Deleted),
                CurrentUserReaction = viewerUserId.HasValue
                    ? (await context.SchoolPostReactions
                        .Where(reaction =>
                            reaction.PostId == post.Id &&
                            reaction.UserId == viewerUserId.Value &&
                            !reaction.Deleted)
                        .Select(reaction => (SchoolPostReactionTypeEnum?)reaction.ReactionType)
                        .FirstOrDefaultAsync())?.ToString()
                    : null
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
