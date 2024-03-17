using Mdaresna.Doamin.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.CoinsManagement
{
    public class CoinType : BaseModel
    {
        public decimal Value { get; set; }

        public string? Note { get; set; }
    }
}
