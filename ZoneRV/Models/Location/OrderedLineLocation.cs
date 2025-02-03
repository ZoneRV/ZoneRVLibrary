using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZoneRV.Models.Location;

public class OrderedLineLocation
{
    public required ProductionLine Line     { get; set; }
    public required LineLocation   Location { get; set; }
    
    public required decimal Order { get; set; }

    [JsonIgnore] public LineLocationType Type 
        => Order == decimal.MinValue ? LineLocationType.PreProduction : 
           Order == decimal.MaxValue ? LineLocationType.PostProduction : 
           LineLocationType.Production;
    
    public static bool operator <(OrderedLineLocation first, OrderedLineLocation second)
    {
        return first.Order < second.Order;
    }
    
    public static bool operator >(OrderedLineLocation first, OrderedLineLocation second)
    {
        return first.Order > second.Order;
    }
}