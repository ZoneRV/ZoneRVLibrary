using System.Data;
using Dapper;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using Serilog;

namespace ZoneRV.DataAccess.DbAccess;

public class MySqlDataAccess : SqlDataAccess
{
    private readonly IConfiguration _config;

    public MySqlDataAccess(IConfiguration config)
    {
        _config = config;
    }
    
    internal override async Task<IEnumerable<T>> LoadData<T, U>(string storedProcedure, U parameters, string connectionId = "Default")
    {
        using IDbConnection connection = new MySqlConnection(_config.GetConnectionString(connectionId));
            
        IEnumerable<T> results = await ExecuteSqlTaskWithRetry(connection.QueryAsync<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure));

        IEnumerable<T> loadData = results.ToList();
            
        Log.Logger.Debug("{storedProcedure} executed. {results} rows returned.", storedProcedure, loadData.Count());
            
        return loadData;
    }

    internal override async Task SaveData<T>(string storedProcedure, T parameters, string connectionId = "Default")
    {
        using IDbConnection connection = new MySqlConnection(_config.GetConnectionString(connectionId));

        int rowsAffected = await ExecuteSqlTaskWithRetry(connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure));

        Log.Logger.Debug("{storedProcedure} executed. {rowsAffected} rows affected.", storedProcedure, rowsAffected);
    }
}