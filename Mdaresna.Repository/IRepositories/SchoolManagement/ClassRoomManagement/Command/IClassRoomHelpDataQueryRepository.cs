using Mdaresna.Doamin.DTOs.ClassRoom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.SchoolManagement.ClassRoomManagement.Command
{
    public interface IClassRoomHelpDataQueryRepository
    {
        public Task<ClassRoomHelpDataDTO> GetClassRoomHrlpDataAsync(Guid SchoolId);
    }
}
