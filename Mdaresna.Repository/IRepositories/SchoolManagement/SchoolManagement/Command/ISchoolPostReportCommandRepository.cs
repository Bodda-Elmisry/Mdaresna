using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Repository.IRepositories.Base;
using System;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IRepositories.SchoolManagement.SchoolManagement.Command
{
    public interface ISchoolPostReportCommandRepository : IBaseCommandRepository<SchoolPostReport>
    {
        Task<bool> DeletePostReportsAsync(Guid postId);
    }
}
