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
    public class StudentExamTransactionRepository : IStudentTransactionManagerRepository
    {
        private readonly AppDbContext context;

        public StudentExamTransactionRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<TransactionStudentParentDeviceResultDTO>> GetTransactionSTudentsParentsDevicesAsync(IEnumerable<Guid> studentsIds)
        {
            var result = (from csm in context.ClassRoomStudentExams
                          join s in context.Students on csm.StudentId equals s.Id
                          join sp in context.StudentParents on s.Id equals sp.StudentId
                          join p in context.Users on sp.ParentId equals p.Id
                          join pd in context.UserDevices on sp.ParentId equals pd.UserId
                          where studentsIds.Contains(csm.StudentId)
                          select new TransactionStudentParentDeviceResultDTO
                          {
                              TransactionId = csm.ExamId,
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
