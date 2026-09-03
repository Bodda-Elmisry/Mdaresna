using Mdaresna.Doamin.DTOs.SchoolManagement;
using Mdaresna.Doamin.Enums;

namespace Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command
{
    public interface ISchoolPostReactionService
    {
        Task<SchoolPostReactionActionResultDTO?> SetReactionAsync(
            Guid postId,
            Guid userId,
            SchoolPostReactionTypeEnum reactionType,
            bool includeSchoolMembers);

        Task<SchoolPostReactionActionResultDTO?> RemoveReactionAsync(
            Guid postId,
            Guid userId,
            bool includeSchoolMembers);

        Task<IEnumerable<SchoolPostReactionUserResultDTO>?> GetReactionsAsync(
            Guid postId,
            Guid userId,
            bool includeSchoolMembers,
            int pageNumber);
    }
}
