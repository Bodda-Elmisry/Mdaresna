using Mdaresna.Doamin.Enums;
using Mdaresna.Infrastructure.Repositories.TransactionsManagement;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.IRepositories.TransactionsManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Factories
{
    public class StudentTransactionsFactory : IStudentTransactionsFactory
    {
        private readonly IServiceProvider serviceProvider;

        public StudentTransactionsFactory(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        public IStudentTransactionManagerRepository GetProvider(StudentTransactionProvidersEnum provider)
        {
            return provider switch
            {
                StudentTransactionProvidersEnum.Activity => (IStudentTransactionManagerRepository)serviceProvider.GetService(typeof(StudentActivityTransactionRepository)),
                StudentTransactionProvidersEnum.Assignment => (IStudentTransactionManagerRepository)serviceProvider.GetService(typeof(StudentAssignementTransactionRepository)),
                StudentTransactionProvidersEnum.Exam => (IStudentTransactionManagerRepository)serviceProvider.GetService(typeof(StudentExamTransactionRepository)),
                StudentTransactionProvidersEnum.Attendance => (IStudentTransactionManagerRepository)serviceProvider.GetService(typeof(StudentAttendanceTransactionRepository)),
                StudentTransactionProvidersEnum.Note => (IStudentTransactionManagerRepository)serviceProvider.GetService(typeof(StudentNoteTransactionRepository)),
                _ => throw new ArgumentException("Invalid student transaction type")
            };
        }
    }
}
