using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Infrastructure.BServices.ReportingManagement;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.IBServices.ReportingManagement;
using Mdaresna.Repository.IFactories;
using Mdaresna.Repository.ReportingDB.IFactories;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Infrastructure.Factories;

public class StudentReportFactory : IStudentReportFactory
{
    private readonly IServiceProvider serviceProvider;
    private readonly IReportingUnitOfWorkFactory reportingUnitOfWorkFactory;

    public StudentReportFactory(
        IServiceProvider serviceProvider,
        IReportingUnitOfWorkFactory reportingUnitOfWorkFactory)
    {
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        this.reportingUnitOfWorkFactory = reportingUnitOfWorkFactory
            ?? throw new ArgumentNullException(nameof(reportingUnitOfWorkFactory));
    }

    public IStudentReportGenerator GetReportProvider(StudentReportTypesEnum reportType)
    {
        try
        {
            var result = reportType switch
            {
                StudentReportTypesEnum.Monthly => (IStudentReportGenerator)serviceProvider.GetService(typeof(StudentMonthelyReportGenerator)),
                _ => throw new ArgumentException("Invalid report type")
            };

            return result;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error retrieving reporting service provider", ex);
        }
    }

    public async Task<int> GenerateAsync(
        ReportQueue queue,
        string schoolConnectionString,
        string reportConnectionString,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(queue);

        if (string.IsNullOrWhiteSpace(schoolConnectionString))
        {
            throw new ArgumentException("School connection string is required.", nameof(schoolConnectionString));
        }

        if (string.IsNullOrWhiteSpace(reportConnectionString))
        {
            throw new ArgumentException("Report connection string is required.", nameof(reportConnectionString));
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(schoolConnectionString)
            .Options;

        await using var context = new AppDbContext(options);
        var reportProvider = CreateReportProvider(
            queue.ReportType,
            context,
            reportingUnitOfWorkFactory,
            reportConnectionString);

        return await reportProvider.GenerateAsync(queue, cancellationToken);
    }

    private static IStudentReportGenerator CreateReportProvider(
        StudentReportTypesEnum reportType,
        AppDbContext context,
        IReportingUnitOfWorkFactory reportingUnitOfWorkFactory,
        string reportConnectionString)
    {
        return reportType switch
        {
            StudentReportTypesEnum.Monthly => new StudentMonthelyReportGenerator(
                context,
                reportingUnitOfWorkFactory,
                reportConnectionString),
            _ => throw new ArgumentException("Invalid report type", nameof(reportType))
        };
    }
}
