using Mdaresna.Doamin.Models.SettingsManagement;

namespace Mdaresna.Repository.IBServices.ReportingManagement;

public interface IStudentReportGenerator
{
    Task<int> GenerateAsync(ReportQueue queue);

    Task<int> GenerateAsync(ReportQueue queue, CancellationToken cancellationToken);
}
