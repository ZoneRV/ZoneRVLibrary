using ZoneRV.Models.Enums;
using ZoneRV.Models.Location;
using ZoneRV.Tests.TestObjects;

namespace ZoneRV.Tests.Location;

public class LocationsTests
{
    private ProductionLocation _g2B1 = TestLocations.LocationFactory.Locations.GetBay(ProductionLine.Gen2, 1)!;
    private ProductionLocation _g2B2 = TestLocations.LocationFactory.Locations.GetBay(ProductionLine.Gen2, 2)!;
    private ProductionLocation _g2B3 = TestLocations.LocationFactory.Locations.GetBay(ProductionLine.Gen2, 3)!;
    private ProductionLocation _g2B4 = TestLocations.LocationFactory.Locations.GetBay(ProductionLine.Gen2, 4)!;
    private ProductionLocation _g2B5 = TestLocations.LocationFactory.Locations.GetBay(ProductionLine.Gen2, 5)!;
    private ProductionLocation _g2B6 = TestLocations.LocationFactory.Locations.GetBay(ProductionLine.Gen2, 6)!;
    private ProductionLocation _g2B7 = TestLocations.LocationFactory.Locations.GetBay(ProductionLine.Gen2, 7)!;
    
    private ProductionLocation _expoB1 = TestLocations.LocationFactory.Locations.GetBay(ProductionLine.Expo, 1)!;
    private ProductionLocation _expoB2 = TestLocations.LocationFactory.Locations.GetBay(ProductionLine.Expo, 2)!;
    private ProductionLocation _expoB3 = TestLocations.LocationFactory.Locations.GetBay(ProductionLine.Expo, 3)!;
    private ProductionLocation _expoB4 = TestLocations.LocationFactory.Locations.GetBay(ProductionLine.Expo, 4)!;
    private ProductionLocation _expoB5 = TestLocations.LocationFactory.Locations.GetBay(ProductionLine.Expo, 5)!;
    
    [Fact]
    public void PositionOperatorTests()
    {
        Assert.True(_g2B1 < _g2B2);
        Assert.True(_g2B4 > _g2B2);
        
        Assert.True(_g2B1 == _g2B1);
        
        Assert.True(_g2B1 != _g2B2);
        Assert.True(_g2B1 != _expoB1);
        
        Assert.False(_g2B1 > _g2B2);
        Assert.False(_g2B4 < _g2B2);
        
        Assert.False(_g2B1 != _g2B1);
        
        Assert.False(_g2B1 == _g2B2);
        Assert.False(_g2B1 == _expoB1);
    }

    [Fact]
    public void PositionEqualityTests()
    {
        Assert.Equal(_g2B1, _g2B1);
        Assert.Equal(_expoB3, _expoB3);
        
        Assert.NotEqual(_expoB3, _expoB2);
        Assert.NotEqual(_expoB3, _g2B3);
    }

    [Fact]
    public void AllHashesUnique()
    {
        var hashes = TestLocations.LocationFactory.Locations.Select(x => x.GetHashCode()).ToList();
        
        Assert.Equal(hashes.Count, hashes.Distinct().Count());
    }
}