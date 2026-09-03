using Mdaresna.Doamin.DTOs.Common;
using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Helpers;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Helpers;
using Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Mdaresna.Infrastructure.Services.SchoolManagement.SchoolManagement.Command
{
    public class SchoolPostReactionService : ISchoolPostReactionService
    {
        private readonly AppDbContext context;
        private readonly AppSettingDTO appSettings;

        public SchoolPostReactionService(
            AppDbContext context,
            IOptions<AppSettingDTO> appSettings)
        {
            this.context = context;
            this.appSettings = appSettings.Value;
        }

        public async Task<SchoolPostReactionActionResultDTO?> SetReactionAsync(
            Guid postId,
            Guid userId,
            SchoolPostReactionTypeEnum reactionType,
            bool includeSchoolMembers)
        {
            if (!await CanAccessPostAsync(postId, userId, includeSchoolMembers))
            {
                return null;
            }

            var existingReaction = await context.SchoolPostReactions
                .FirstOrDefaultAsync(reaction =>
                    reaction.PostId == postId && reaction.UserId == userId);

            if (existingReaction == null)
            {
                context.SchoolPostReactions.Add(new SchoolPostReaction
                {
                    Id = DataGenerationHelper.GenerateRowId(),
                    PostId = postId,
                    UserId = userId,
                    ReactionType = reactionType,
                    CreateDate = DateTime.Now,
                    LastModifyDate = DateTime.Now
                });
            }
            else
            {
                existingReaction.ReactionType = reactionType;
                existingReaction.Deleted = false;
                existingReaction.LastModifyDate = DateTime.Now;
            }

            try
            {
                await context.SaveChangesAsync();
            }
            catch (DbUpdateException exception) when (
                existingReaction == null && IsUniqueConstraintViolation(exception))
            {
                context.ChangeTracker.Clear();
                var concurrentReaction = await context.SchoolPostReactions
                    .FirstAsync(reaction =>
                        reaction.PostId == postId && reaction.UserId == userId);
                concurrentReaction.ReactionType = reactionType;
                concurrentReaction.Deleted = false;
                concurrentReaction.LastModifyDate = DateTime.Now;
                await context.SaveChangesAsync();
            }

            return await GetReactionSummaryAsync(postId, userId);
        }

        public async Task<SchoolPostReactionActionResultDTO?> RemoveReactionAsync(
            Guid postId,
            Guid userId,
            bool includeSchoolMembers)
        {
            if (!await CanAccessPostAsync(postId, userId, includeSchoolMembers))
            {
                return null;
            }

            var existingReaction = await context.SchoolPostReactions
                .FirstOrDefaultAsync(reaction =>
                    reaction.PostId == postId &&
                    reaction.UserId == userId &&
                    !reaction.Deleted);

            if (existingReaction != null)
            {
                existingReaction.Deleted = true;
                existingReaction.LastModifyDate = DateTime.Now;
                await context.SaveChangesAsync();
            }

            return await GetReactionSummaryAsync(postId, userId);
        }

        public async Task<IEnumerable<SchoolPostReactionUserResultDTO>?> GetReactionsAsync(
            Guid postId,
            Guid userId,
            bool includeSchoolMembers,
            int pageNumber)
        {
            if (!await CanAccessPostAsync(postId, userId, includeSchoolMembers))
            {
                return null;
            }

            var pageSize = appSettings.PageSize ?? 30;
            pageNumber = pageNumber <= 0 ? 1 : pageNumber;

            var reactions = await context.SchoolPostReactions
                .AsNoTracking()
                .Where(reaction =>
                    reaction.PostId == postId &&
                    !reaction.Deleted &&
                    !reaction.User.Deleted)
                .OrderByDescending(reaction => reaction.LastModifyDate ?? reaction.CreateDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(reaction => new
                {
                    reaction.UserId,
                    reaction.User.FirstName,
                    reaction.User.LastName,
                    reaction.User.ImageUrl,
                    reaction.ReactionType,
                    ReactionDate = reaction.LastModifyDate ?? reaction.CreateDate ?? DateTime.Now
                })
                .ToListAsync();

            return reactions.Select(reaction => new SchoolPostReactionUserResultDTO
            {
                UserId = reaction.UserId,
                UserName = $"{reaction.FirstName} {reaction.LastName}",
                UserImageUrl = string.IsNullOrEmpty(reaction.ImageUrl)
                    ? null
                    : $"{SettingsHelper.GetAppUrl()}/{reaction.ImageUrl.Replace("\\", "/")}",
                ReactionType = reaction.ReactionType.ToString(),
                ReactionDate = reaction.ReactionDate
            });
        }

        private async Task<bool> CanAccessPostAsync(
            Guid postId,
            Guid userId,
            bool includeSchoolMembers)
        {
            return userId != Guid.Empty &&
                await context.Users.AnyAsync(user =>
                    user.Id == userId && !user.Deleted) &&
                await context.SchoolPosts
                .AsNoTracking()
                .AnyAsync(post =>
                    post.Id == postId &&
                    !post.Deleted &&
                    post.ModerationStatus == SchoolPostModerationStatusEnum.Approved &&
                    (post.Visibility == SchoolPostVisibilityEnum.Public ||
                     (includeSchoolMembers &&
                      post.Visibility == SchoolPostVisibilityEnum.SchoolMembers)) &&
                    !context.UserBlocks.Any(block =>
                        !block.Deleted &&
                        block.BlockerUserId == userId &&
                        block.BlockedUserId == post.PosterId));
        }

        private async Task<SchoolPostReactionActionResultDTO> GetReactionSummaryAsync(
            Guid postId,
            Guid userId)
        {
            var reactionsCount = await context.SchoolPostReactions
                .AsNoTracking()
                .CountAsync(reaction =>
                    reaction.PostId == postId &&
                    !reaction.Deleted &&
                    !reaction.User.Deleted);

            var currentUserReaction = await context.SchoolPostReactions
                .AsNoTracking()
                .Where(reaction =>
                    reaction.PostId == postId &&
                    reaction.UserId == userId &&
                    !reaction.Deleted)
                .Select(reaction => (SchoolPostReactionTypeEnum?)reaction.ReactionType)
                .FirstOrDefaultAsync();

            return new SchoolPostReactionActionResultDTO
            {
                ReactionsCount = reactionsCount,
                CurrentUserReaction = currentUserReaction?.ToString()
            };
        }

        private static bool IsUniqueConstraintViolation(DbUpdateException exception)
        {
            return exception.InnerException is SqlException sqlException &&
                (sqlException.Number == 2601 || sqlException.Number == 2627);
        }
    }
}
