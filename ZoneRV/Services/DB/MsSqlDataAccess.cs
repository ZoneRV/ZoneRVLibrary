using System.Data;
using System.Data.SqlClient;
using Dapper;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace ZoneRV.Services.DB;

public class MsSqlDataAccess : SqlDataAccess
{
    private readonly IConfiguration _config;

    public MsSqlDataAccess(IConfiguration config)
    {
        _config = config;
    }
    
    public override async Task<IEnumerable<T>> LoadData<T, U>(string storedProcedure, U parameters, string connectionId = "Default")
    {
        using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionId));
            
        IEnumerable<T> results = await ExecuteSqlTaskWithRetry(connection.QueryAsync<T>(storedProcedure, parameters, commandType: CommandType.StoredProcedure));

        IEnumerable<T> loadData = results.ToList();
            
        Log.Logger.Debug("{storedProcedure} executed. {results} rows returned.", storedProcedure, loadData.Count());
            
        return loadData;
    }

    public override async Task SaveData<T>(string storedProcedure, T parameters, string connectionId = "Default")
    {
        using IDbConnection connection = new SqlConnection(_config.GetConnectionString(connectionId));

        int rowsAffected = await ExecuteSqlTaskWithRetry(connection.ExecuteAsync(storedProcedure, parameters, commandType: CommandType.StoredProcedure));

        Log.Logger.Debug("{storedProcedure} executed. {rowsAffected} rows affected.", storedProcedure, rowsAffected);
    }
}