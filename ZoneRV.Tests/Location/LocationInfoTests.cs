using ZoneRV.Models.Enums;
using ZoneRV.Models.Location;
using ZoneRV.Tests.Objects;

namespace ZoneRV.Tests.Location;

public class LocationInfoTests
{
    [Fact]
    public void AddLocationChangesThrows()
    {
        var fullHistory = ProductionTestData.GetLocationInfo(ProductionTestData.Gen2, 1);
        var halfHistory = ProductionTestData.GetLocationInfo(ProductionTestData.Gen2, .5f);
        
        // Location Exists
        Assert.Throws<ArgumentException>(
            "location",
            () => fullHistory.AddPositionChange(DateTimeOffset.MaxValue, LocationFactory.PreProduction));
        
        // Cant add Non bays (unless pre or post-production)
        Assert.Throws<ArgumentException>(
            "Type",
            () =>fullHistory.AddPositionChange(DateTimeOffset.MaxValue, ProductionTestData.LocationFactory.Locations.First(x => x.Type == ProductionLocationType.Module)));
        
        // Cant add different production lines
        Assert.Throws<ArgumentException>(
            "ProductionLine",
            () => fullHistory.AddPositionChange(DateTimeOffset.MaxValue,
                ProductionTestData.LocationFactory.Locations.First(x => x.ProductionLine == ProductionTestData.Expo && x.Type == ProductionLocationType.Bay)));

        // Cant add move with already existing date
        Assert.Throws<ArgumentException>(
            "date",
            () => halfHistory.AddPositionChange(halfHistory.LocationHistory.Min(x => x.moveDate),
                ProductionTestData.LocationFactory.Locations.Where(x => x.ProductionLine == ProductionTestData.Gen2 && x.Type == ProductionLocationType.Bay).MaxBy(x => x.Order)));
    }
}