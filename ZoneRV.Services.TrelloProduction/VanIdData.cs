using ZoneRV.Models.DB;
using ZoneRV.Services.DB;

namespace ZoneRV.DataAccess.Data;

public class VanIdData
{
    private readonly SqlDataAccess _db;

    public VanIdData(SqlDataAccess db)
    {
        _db = db;
    }

    public Task<IEnumerable<VanID>> GetIds()
        => _db.LoadData<VanID, dynamic>("dbo.spVanId_GetAll", new { });

    public async Task<VanID?> GetId(string vanName)
    {
        var results = await _db.LoadData<VanID, dynamic>("dbo.spVanId_get", new { VanName = vanName });

        return results.FirstOrDefault();
    }

    public async Task InsertVanId(VanID vanId)
        => await _db.SaveData("dbo.spVanId_Insert", vanId);

    public async Task UpdateVanId(VanID vanId)
        => await _db.SaveData("dbo.spVanId_Update", vanId);

    public async Task DeleteVanId(string vanName)
        => await _db.SaveData("dbo.SpVanId_Delete", new { VanName = vanName });
        
    public async Task BlockVan(string vanName, bool blocked)
        => await _db.SaveData("dbo.SpVanId_Block", new { VanName = vanName, Blocked = blocked });
}