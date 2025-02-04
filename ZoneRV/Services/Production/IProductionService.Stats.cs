using System.Diagnostics.CodeAnalysis;

namespace ZoneRV.Services.Production;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract partial class IProductionService
{
    public int VansInCarPark 
        => Vans.Values.Count(x =>
            x.HandoverDate.HasValue &&
            x.HandoverState == HandoverState.UnhandedOver &&
            x.OrderedLineLocationInfo.CurrentLocation is not null && 
            x.OrderedLineLocationInfo.CurrentLocation.Location.Type == ProductionLocationType.Finishing);
    
    public int VansHandoverOverdue
        => Vans.Values.Count(x =>
            x.HandoverDate.HasValue &&
            x.HandoverState != HandoverState.HandedOver &&
            x.HandoverDate.Value < DateTimeOffset.Now);
    
    public int VansHandedOver
        => Vans.Values.Count(x => x.HandoverState == HandoverState.HandedOver);
}