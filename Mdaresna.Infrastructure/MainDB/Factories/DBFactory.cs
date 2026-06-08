using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.MainDB.Enums;
using Mdaresna.Infrastructure.BServices.Common;
using Mdaresna.Infrastructure.MainDB.Services;
using Mdaresna.Repository.IBServices.Common;
using Mdaresna.Repository.MainDB.IFactories;
using Mdaresna.Repository.MainDB.IServices;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Mdaresna.Infrastructure.MainDB.Factories;

public class DBFactory : IDBFactory
{
    private readonly IServiceProvider serviceProvider;

    public DBFactory(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    public string GetConnectionString(DBTypeEnum dBType, string dBSource, string dBUser, string dBPassword, string dBCatlog, string? dBPort = null)
    {
        try
        {
            var result = dBType switch
            {
                DBTypeEnum.SQLServer => (IDBService)serviceProvider.GetService(typeof(DBSQLServerService)),
                DBTypeEnum.PostGreSQL => (IDBService)serviceProvider.GetService(typeof(DBPostGreSQLService)),
                _ => throw new ArgumentException("Invalid Database Type")
            };

            return result.GenerateConnectionString(dBSource, dBUser, dBPassword, dBCatlog, dBPort);
        }
        catch (Exception ex)
        {
            // Log the exception or handle it as needed
            throw new InvalidOperationException("Error getting data base connection string", ex);
        }
    }
}
