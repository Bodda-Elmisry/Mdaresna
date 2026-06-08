using Mdaresna.Doamin.MainDB.Enums;

namespace Mdaresna.Repository.MainDB.IFactories;

public interface IDBFactory
{
    string GetConnectionString(DBTypeEnum dBType, string dBSource, string dBUser, string dBPassword, string dBCatlog, string? dBPort = null);
}
