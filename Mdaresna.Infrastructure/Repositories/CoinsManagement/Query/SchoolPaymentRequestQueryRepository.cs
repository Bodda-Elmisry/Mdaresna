using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.Repositories.Base;
using Mdaresna.Repository.IRepositories.CoinsManagement.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.CoinsManagement.Query
{
    public class SchoolPaymentRequestQueryRepository : BaseQueryRepository<SchoolPaymentRequest>, ISchoolPaymentRequestQueryRepository
    {
       public SchoolPaymentRequestQueryRepository(AppDbContext context) : base(context)
        {
        }
    }
}
