namespace Mdaresna.Doamin.Models.Base
{
    public class AuditBase
    {
        
        public DateTime? CreateDate { get; set; }
        public DateTime? LastModifyDate { get; set; }
        public bool Deleted { get; set; } = false;
    }
}
