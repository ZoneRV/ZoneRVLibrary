using System.Diagnostics.CodeAnalysis;
using ZoneRV.Models.UpdateModels;

namespace ZoneRV.Services.Production;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract partial class IProductionService
{
    public static event EventHandler<VanUpdated>?  VanUpdated;
    public static event EventHandler<VanUpdated>?  VanAddedToProduction;

    public void UpdateCheck(object? sender, CheckUpdated data)
    {
        throw new NotImplementedException();
    }
    
    public void UpdateCheckList(object? sender, ChecklistUpdated data)
    {
        throw new NotImplementedException();
    }
    
    public void UpdateCard(object? sender, CardUpdated data)
    {
        throw new NotImplementedException();
    }
    
    
}