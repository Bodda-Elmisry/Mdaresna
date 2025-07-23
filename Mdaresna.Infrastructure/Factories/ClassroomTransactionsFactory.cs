using Mdaresna.Doamin.Enums;
using Mdaresna.Infrastructure.BServices.Common;
using Mdaresna.Infrastructure.Repositories.TransactionsManagement;
using Mdaresna.Repository.IBServices.Common;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IRepositories.TransactionsManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Factories
{
    public class ClassroomTransactionsFactory : IClassroomTransactionsFactory
    {
        private readonly IServiceProvider serviceProvider;

        public ClassroomTransactionsFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IClassroomTransactionManagerRepository GetProvider(ClassroomTransactionProvidersEnum provider)
        {
            return provider switch
            {
                ClassroomTransactionProvidersEnum.Activity => (IClassroomTransactionManagerRepository)serviceProvider.GetService(typeof(ClassroomActivityTransactionRepository)),
                ClassroomTransactionProvidersEnum.Assignment => (IClassroomTransactionManagerRepository)serviceProvider.GetService(typeof(ClassroomAssignementTransactionRepository)),
                ClassroomTransactionProvidersEnum.Exam => (IClassroomTransactionManagerRepository)serviceProvider.GetService(typeof(ClassroomExamTransactionRepository)),
                _ => throw new ArgumentException("Invalid classroom transaction type")
            };
        }
    }
}
