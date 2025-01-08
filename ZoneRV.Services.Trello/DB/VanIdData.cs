using ZoneRV.Services.DB;
using ZoneRV.Services.Trello.Models;

namespace ZoneRV.Services.Trello.DB;

public class VanIdData
{
    private readonly SqlDataAccess _db;

    public VanIdData(SqlDataAccess db)
    {
        _db = db;
    }

    public Task<IEnumerable<VanId>> GetIds()
        => _db.LoadData<VanId, dynamic>("dbo.spVanId_GetAll", new { });

    public async Task<VanId?> GetId(string vanName)
    {
        var results = await _db.LoadData<VanId, dynamic>("dbo.spVanId_get", new { VanName = vanName });

        return results.FirstOrDefault();
    }

    public async Task InsertVanId(VanId vanId)
        => await _db.SaveData("dbo.spVanId_Insert", vanId);

    public async Task UpdateVanId(VanId vanId)
        => await _db.SaveData("dbo.spVanId_Update", vanId);

    public async Task DeleteVanId(string vanName)
        => await _db.SaveData("dbo.SpVanId_Delete", new { VanName = vanName });
        
    public async Task BlockVan(string vanName, bool blocked)
        => await _db.SaveData("dbo.SpVanId_Block", new { VanName = vanName, Blocked = blocked });
}