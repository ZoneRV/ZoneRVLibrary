using System.Diagnostics.CodeAnalysis;
using ZoneRV.Models.UpdateModels;

namespace ZoneRV.Services.Production;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract partial class IProductionService
{
    public event EventHandler<VanUpdated>?  VanUpdated;
    public event EventHandler<CardUpdated>?  CardUpdated;
    public event EventHandler<VanUpdated>?  VanAddedToProduction;

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
        CardUpdated?.Invoke(this, data);
        throw new NotImplementedException();
    }
    
    
}