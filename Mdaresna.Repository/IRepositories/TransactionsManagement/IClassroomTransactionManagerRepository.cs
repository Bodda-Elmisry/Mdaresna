using Mdaresna.Doamin.DTOs.StudentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.TransactionsManagement
{
    public interface IClassroomTransactionManagerRepository
    {
        Task<IEnumerable<TransactionStudentParentDeviceResultDTO>> GetTransactionSTudentsParentsDevicesAsync(Guid transactionId);
    }
}
