using Mdaresna.Doamin.DTOs.StudentManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.IRepositories.TransactionsManagement;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.Repositories.TransactionsManagement
{
    public class ClassroomExamTransactionRepository : IClassroomTransactionManagerRepository
    {
        private readonly AppDbContext context;

        public ClassroomExamTransactionRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<TransactionStudentParentDeviceResultDTO>> GetTransactionSTudentsParentsDevicesAsync(Guid transactionId)
        {
            var result = (from ce in context.ClassRoomExams
                          join cse in context.ClassRoomStudentExams on ce.Id equals cse.ExamId
                          join s in context.Students on cse.StudentId equals s.Id
                          join sp in context.StudentParents on s.Id equals sp.StudentId
                          join p in context.Users on sp.ParentId equals p.Id
                          join pd in context.UserDevices on sp.ParentId equals pd.UserId
                          where ce.Id == transactionId
                          select new TransactionStudentParentDeviceResultDTO
                          {
                              TransactionId = ce.Id,
                              TransactionType = "Exam",
                              StudentId = s.Id,
                              StudentName = s.FirstName + " " + s.LastName,
                              ParentId = p.Id,
                              ParentName = p.FirstName + " " + p.LastName,
                              DeviceId = pd.DeviceId,
                              Platform = pd.Platform.ToString(),
                              FcmTocken = pd.FcmToken,
                              SignalRConnectionId = pd.SignalRConnectionId,
                              LastSeen = pd.LastSeen
                          }).AsQueryable();

            return await result.ToListAsync();
        }
    }
}
