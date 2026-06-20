namespace Mdaresna.Doamin.DTOs.SchoolManagement
{
    public class SchoolResultDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string About { get; set; }
        public string Vesion { get; set; }
        public bool? Active { get; set; }
        public string ImageUrl { get; set; }
        public Guid SchoolTypeId { get; set; }
        public string SchoolTypeName { get; set; }
        public Guid? CoinTypeId { get; set; }
        public string CoinTypeName { get; set; }
        public int AvailableCoins { get; set; }
        public Guid SchoolAdminId { get; set; }
        public string SchoolAdminName { get; set; }
        public IEnumerable<string> SchoolImages { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
