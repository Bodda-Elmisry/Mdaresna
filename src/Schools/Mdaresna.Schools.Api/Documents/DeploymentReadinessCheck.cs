using System.Data.Common;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Mdaresna.Schools.Api.Documents;

// Check the provisioning database, not every tenant database on every health request.
internal sealed class DeploymentReadinessCheck(IConfiguration configuration, IWebHostEnvironment environment, ISchoolDocumentStorage storage) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var cipher = UploadEncryptionConfiguration.Create(configuration);
            using (var encryptedProbe = new MemoryStream())
                await cipher.EncryptAsync(Stream.Null, encryptedProbe, "readiness", cancellationToken);
            if (storage is R2SchoolDocumentStorage r2)
                await r2.CheckReadinessAsync(cancellationToken);
            else
            {
                var root = configuration["SchoolDocuments:StorageRoot"] ?? "App_Data/school-documents";
                var path = Path.IsPathRooted(root) ? root : Path.Combine(environment.ContentRootPath, root);
                Directory.CreateDirectory(path);
                // Empty, disposable probe: no user bytes and no plaintext document are written.
                var probe = Path.Combine(path, $".readiness-{Guid.NewGuid():N}");
                await using (var file = new FileStream(probe, FileMode.CreateNew, FileAccess.Write, FileShare.None, 1, FileOptions.DeleteOnClose)) { }
            }
            var value = configuration["SchoolProvisioningDatabase:AdminConnectionString"];
            if (string.IsNullOrWhiteSpace(value)) return HealthCheckResult.Unhealthy("Provisioning database configuration is missing.");
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(3));
            await using DbConnection connection = configuration["SchoolProvisioningDatabase:Provider"] == "SqlServer"
                ? new Microsoft.Data.SqlClient.SqlConnection(value)
                : new Npgsql.NpgsqlConnection(value);
            await connection.OpenAsync(timeout.Token);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            command.CommandTimeout = 3;
            await command.ExecuteScalarAsync(timeout.Token);
            return HealthCheckResult.Healthy();
        }
        catch
        {
            return HealthCheckResult.Unhealthy("Database connectivity, document storage or upload encryption configuration is not ready.");
        }
    }
}
