using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.CoinsManagement.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.CoinsManagement.Command
{
    public class PaymentTypeCommandRepository : BaseCommandRepository<PaymentType>, IPaymentTypeCommandRepository
    {
        public PaymentTypeCommandRepository(AppDbContext context) : base(context)
        {
        }
    }
}
