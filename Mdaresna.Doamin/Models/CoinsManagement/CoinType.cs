using Mdaresna.Doamin.Models.Base;

namespace Mdaresna.Doamin.Models.CoinsManagement
{
    public class CoinType : BaseModel
    {
        public decimal Value { get; set; }

        public string? Note { get; set; }
    }
}
