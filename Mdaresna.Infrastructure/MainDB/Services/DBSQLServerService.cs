using Mdaresna.Repository.MainDB.IServices;

namespace Mdaresna.Infrastructure.MainDB.Services;

public class DBSQLServerService : IDBService
{
    public string GenerateConnectionString(string dBSource, string dBUser, string dBPassword, string dBCatlog, string? dBPort = null)
    {
        return $"Data Source={dBSource};uid={dBUser}; pwd={dBPassword};Initial Catalog={dBCatlog}; TrustServerCertificate=True";
    }
}
