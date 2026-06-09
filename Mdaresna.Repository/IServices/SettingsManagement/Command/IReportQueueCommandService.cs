using Mdaresna.Doamin.DTOs.SettingsManagement;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Repository.IServices.Base;

namespace Mdaresna.Repository.IServices.SettingsManagement.Command
{
    public interface IReportQueueCommandService : IBaseCommandService<ReportQueue>
    {
        Task<bool> MarkStartedAsync(Guid id);

        Task<bool> MarkCompletedAsync(Guid id, int? affectedRows = null, string? notes = null);

        Task<bool> MarkFailedAsync(Guid id, string errors, int? affectedRows = null, string? notes = null);

        Task<RequestMonthReportResponseDTO> RequestMonthReportAsync(RequestMonthReportCommandDTO command);

        Task<PublishReportQueueResponseDTO> PublishReportQueueAsync(
            PublishReportQueueCommandDTO command,
            CancellationToken cancellationToken = default);
    }
}
