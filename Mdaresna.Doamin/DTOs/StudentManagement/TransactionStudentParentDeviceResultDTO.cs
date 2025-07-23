using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.StudentManagement
{
    public class TransactionStudentParentDeviceResultDTO
    {
        public Guid TransactionId { get; set; }
        public string TransactionType { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public Guid ParentId { get; set; }
        public string ParentName { get; set; }
        public string DeviceId { get; set; }
        public string Platform { get; set; }
        public string FcmTocken { get; set; }
        public string? SignalRConnectionId { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
