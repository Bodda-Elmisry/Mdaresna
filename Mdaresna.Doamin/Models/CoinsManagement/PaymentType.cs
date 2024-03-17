using Mdaresna.Doamin.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.CoinsManagement
{
    public class PaymentType : BaseModel
    {
        public bool IsActive { get; set; }

        public string? Note { get; set; }
    }
}
