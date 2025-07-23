using Mdaresna.Doamin.DTOs.StudentManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.TransactionsManagement
{
    public interface IStudentTransactionManagerRepository
    {
        Task<IEnumerable<TransactionStudentParentDeviceResultDTO>> GetTransactionSTudentsParentsDevicesAsync(IEnumerable<Guid> studentsIds);
    }
}
