using System.Diagnostics.CodeAnalysis;
using Serilog;
using ZoneRV.Models;

namespace ZoneRV.Services.Production;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract partial class IProductionService
{
    public bool TryGetInfoByName(string name, [NotNullWhen(true)] out SalesOrder? info)
        => TryGetSingleInfo(x => string.Equals(x.Name, name, StringComparison.CurrentCultureIgnoreCase), out info);
    
    public bool TryGetInfoById(string id, [NotNullWhen(true)] out SalesOrder? info)
        => SalesOrders.TryGetValue(id.ToLower(), out info);
    
    public IEnumerable<SalesOrder> GetInfos(Func<SalesOrder, bool> predicate)
        => SalesOrders.Values.Where(predicate);

    public bool TryGetSingleInfo(Func<SalesOrder, bool> predicate, [NotNullWhen(true)] out SalesOrder? info)
    {
        var infos = GetInfos(predicate).ToList();
        
        if(infos.Count == 1)
        {
            info = infos.First();
            return true;
        }
        
        info = null;
        
        if(infos.Count == 0)
        {
            Log.Logger.Warning("Could not find any vans that match the predicate {predicate}", predicate); // TODO: fix log
            return false;
        }
        else
        {
            Log.Logger.Warning("Found multiple vans that match the predicate {predicate}", predicate); // TODO: fix log
            return false;
        }
    }
}