using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Repository.IServices.Base;
using System;
using System.Threading.Tasks;

namespace Mdaresna.Repository.IServices.SchoolManagement.SchoolManagement.Command
{
    public interface ISchoolPostReportCommandService : IBaseCommandService<SchoolPostReport>
    {
        Task<bool> DeletePostReportsByPostIdAsync(Guid postId);
    }
}
