using ZoneRV.Services.DB;

namespace ZoneRV.Services;

public class LocationData
{
    private readonly SqlDataAccess _db;

    public LocationData(SqlDataAccess db)
    {
        _db = db;
    }
    
    public Task<IEnumerable<ProductionLocation>> GetLocations(CustomNameSource source)
    {
        switch (source)
        {
            case CustomNameSource.Trello:
                return _db.LoadData<ProductionLocation, dynamic>("trello.spLocationsWithListNames_GetAll", new { });
            
            default:
                throw new NotImplementedException();
        }
    }
}