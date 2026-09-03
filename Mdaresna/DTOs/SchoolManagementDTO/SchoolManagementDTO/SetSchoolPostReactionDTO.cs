using Mdaresna.Doamin.Enums;

namespace Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO
{
    public class SetSchoolPostReactionDTO
    {
        public Guid PostId { get; set; }
        public SchoolPostReactionTypeEnum ReactionType { get; set; } =
            SchoolPostReactionTypeEnum.Like;
    }
}
