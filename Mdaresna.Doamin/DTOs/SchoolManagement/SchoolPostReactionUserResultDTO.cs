namespace Mdaresna.Doamin.DTOs.SchoolManagement
{
    public class SchoolPostReactionUserResultDTO
    {
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string? UserImageUrl { get; set; }
        public string ReactionType { get; set; }
        public DateTime ReactionDate { get; set; }
    }
}
