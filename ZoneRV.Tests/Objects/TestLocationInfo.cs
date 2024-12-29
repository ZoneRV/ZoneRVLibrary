using ZoneRV.Models.Enums;
using ZoneRV.Models.Location;

namespace ZoneRV.Tests.Objects;

public static class TestLocationInfo
{
    static TestLocationInfo()
    {
        var gen2Positions = TestLocations.LocationFactory.Locations.Where(x => x.ProductionLine == ProductionLine.Gen2 && x.Type == ProductionLocationType.Bay).ToList();
        gen2Positions.AddRange([LocationFactory.PreProduction, LocationFactory.PostProduction]);
        gen2Positions = gen2Positions.OrderBy(x => x.LocationOrder).ToList();

        for (int i = 0; i < gen2Positions.Count; i++)
        {
            FullGen2Info.AddPositionChange(DateTimeOffset.Now - TimeSpan.FromDays(i), gen2Positions.ElementAt(i));
            
            if(i < gen2Positions.Count / 2)
                HalfGen2Info.AddPositionChange(DateTimeOffset.Now - TimeSpan.FromDays(i), gen2Positions.ElementAt(i));
        }
    }
    
    public static VanLocationInfo FullGen2Info = new ();
    public static VanLocationInfo HalfGen2Info = new ();
    public static VanLocationInfo EmptyInfo = new ();
}