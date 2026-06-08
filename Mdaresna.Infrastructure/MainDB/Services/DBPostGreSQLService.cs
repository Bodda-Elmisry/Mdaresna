using Mdaresna.Repository.MainDB.IServices;

namespace Mdaresna.Infrastructure.MainDB.Services;

public class DBPostGreSQLService : IDBService
{
    public string GenerateConnectionString(string dBSource, string dBUser, string dBPassword, string dBCatlog, string? dBPort = null)
    {
        if (dBPort == null)
            throw new Exception("Error in connection string");

        return $"Host={dBSource};Port={dBPort};Database={dBCatlog};Username={dBUser};Password={dBPassword}";
    }
}
