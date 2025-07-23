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
    public class ClassroomAssignementTransactionRepository : IClassroomTransactionManagerRepository
    {
        private readonly AppDbContext context;

        public ClassroomAssignementTransactionRepository(AppDbContext context)
        {
            this.context = context;
        }

        public async Task<IEnumerable<TransactionStudentParentDeviceResultDTO>> GetTransactionSTudentsParentsDevicesAsync(Guid transactionId)
        {
            var result = (from ca in context.ClassRoomAssignments
                          join csa in context.ClassRoomStudentAssignments on ca.Id equals csa.AssignmentId
                          join s in context.Students on csa.StudentId equals s.Id
                          join sp in context.StudentParents on s.Id equals sp.StudentId
                          join p in context.Users on sp.ParentId equals p.Id
                          join pd in context.UserDevices on sp.ParentId equals pd.UserId
                          where ca.Id == transactionId
                          select new TransactionStudentParentDeviceResultDTO
                          {
                              TransactionId = ca.Id,
                              TransactionType = "Assignment",
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
