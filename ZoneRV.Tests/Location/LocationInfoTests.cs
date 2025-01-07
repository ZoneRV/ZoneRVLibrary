using ZoneRV.Models.Enums;
using ZoneRV.Models.Location;
using ZoneRV.Tests.Objects;

namespace ZoneRV.Tests.Location;

public class LocationInfoTests
{
    [Fact]
    public void AddLocationChangesThrows()
    {
        // Location Exists
        Assert.Throws<ArgumentException>(
            "location",
            () => TestLocationInfo.FullGen2Info.AddPositionChange(DateTimeOffset.MaxValue, LocationFactory.PreProduction));
        
        // Cant add Non bays (unless pre or post-production)
        Assert.Throws<ArgumentException>(
            "Type",
            () => TestLocationInfo.FullGen2Info.AddPositionChange(DateTimeOffset.MaxValue,
                TestLocations.LocationFactory.Locations.First(x => x.Type == ProductionLocationType.Module)));
        
        // Cant add different production lines
        Assert.Throws<ArgumentException>(
            "ProductionLine",
            () => TestLocationInfo.FullGen2Info.AddPositionChange(DateTimeOffset.MaxValue,
                TestLocations.LocationFactory.Locations.First(x => x.ProductionLine == ProductionLine.Expo && x.Type == ProductionLocationType.Bay)));

        // Cant add move with already existing date
        Assert.Throws<ArgumentException>(
            "date",
            () => TestLocationInfo.HalfGen2Info.AddPositionChange(TestLocationInfo.HalfGen2Info.LocationHistory.First().moveDate,
                TestLocations.LocationFactory.Locations.Last(x => x.ProductionLine == ProductionLine.Gen2 && x.Type == ProductionLocationType.Bay)));
    }
}