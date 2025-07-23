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
    public class StudentNoteTransactionRepository : IStudentTransactionManagerRepository
    {
        private readonly AppDbContext context;

        public StudentNoteTransactionRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<TransactionStudentParentDeviceResultDTO>> GetTransactionSTudentsParentsDevicesAsync(IEnumerable<Guid> studentsIds)
        {
            var result = (from sn in context.studentNotes
                          join s in context.Students on sn.StudentId equals s.Id
                          join sp in context.StudentParents on s.Id equals sp.StudentId
                          join p in context.Users on sp.ParentId equals p.Id
                          join pd in context.UserDevices on sp.ParentId equals pd.UserId
                          where studentsIds.Contains(sn.StudentId)
                          select new TransactionStudentParentDeviceResultDTO
                          {
                              TransactionId = sn.Id,
                              TransactionType = "Note",
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
