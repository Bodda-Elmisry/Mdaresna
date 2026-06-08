using Mdaresna.Doamin.MainDB.Enums;

namespace Mdaresna.Repository.MainDB.IServices;

public interface IDBService
{
    string GenerateConnectionString(string dBSource, string dBUser, string dBPassword, string dBCatlog, string? dBPort = null);
}
