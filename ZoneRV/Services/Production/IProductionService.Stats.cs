using System.Diagnostics.CodeAnalysis;

namespace ZoneRV.Services.Production;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public abstract partial class IProductionService
{
    public int SalesOrdersInCarPark 
        => SalesOrders.Values.Count(x =>
            x.RedlineDate.HasValue &&
            x.HandoverState == HandoverState.UnhandedOver &&
            x.LocationInfo.CurrentLocation is not null && 
            x.LocationInfo.CurrentLocation.Location.LocationType == ProductionLocationType.Finishing);
    
    public int SalesOrdersHandoverOverdue
        => SalesOrders.Values.Count(x =>
            x.RedlineDate.HasValue &&
            x.HandoverState != HandoverState.HandedOver &&
            x.RedlineDate.Value < DateTimeOffset.Now);
    
    public int SalesOrdersHandedOver
        => SalesOrders.Values.Count(x => x.HandoverState == HandoverState.HandedOver);
}