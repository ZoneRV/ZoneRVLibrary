using ZoneRV.Services.DB;

namespace ZoneRV.Services;

public class ProductionDataService
{
    private readonly SqlDataAccess _db;

    public ProductionDataService(SqlDataAccess db)
    {
        _db = db;
    }
    
    public Task<IEnumerable<ProductionLine>> GetProductionLines()
    {
        return _db.LoadData<ProductionLine, dynamic>("production.spLinesWithModels_GetAll", new { });
    }
}