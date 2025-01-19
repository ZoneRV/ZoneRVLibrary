using System.Diagnostics.CodeAnalysis;

namespace ZoneRV.Services.Production;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract partial class IProductionService
{
    public int VansInCarPark 
        => Vans.Values.Count(x =>
            x.HandoverDate.HasValue &&
            x.HandoverState == HandoverState.UnhandedOver &&
            x.LocationInfo.CurrentLocation.Type == ProductionLocationType.Finishing);
    
    public int VansHandoverOverdue
        => Vans.Values.Count(x =>
            x.HandoverDate.HasValue &&
            x.HandoverState == HandoverState.UnhandedOver &&
            x.HandoverDate.Value < DateTimeOffset.Now &&
            x.LocationInfo.CurrentLocation.Type == ProductionLocationType.Finishing);
    
    public int VansHandedOver
        => Vans.Values.Count(x => x.HandoverState == HandoverState.HandedOver);
}