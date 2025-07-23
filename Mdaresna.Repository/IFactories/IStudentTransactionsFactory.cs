using Mdaresna.Doamin.Enums;
using Mdaresna.Repository.IRepositories.TransactionsManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IFactories
{
    public interface IStudentTransactionsFactory
    {
        IStudentTransactionManagerRepository GetProvider(StudentTransactionProvidersEnum provider);
    }
}
