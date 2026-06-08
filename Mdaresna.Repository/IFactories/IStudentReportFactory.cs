using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Repository.IBServices.ReportingManagement;

namespace Mdaresna.Repository.IFactories;

public interface IStudentReportFactory
{
    IStudentReportGenerator GetReportProvider(StudentReportTypesEnum reportType);

    Task<int> GenerateAsync(
        ReportQueue queue,
        string schoolConnectionString,
        string reportConnectionString,
        CancellationToken cancellationToken);
}
