using ZoneRV.Models.Enums;
using ZoneRV.Models.Location;
using ZoneRV.Tests.TestObjects;

namespace ZoneRV.Tests.Location;

public class LocationFactoryTests
{
    [Theory]
    [InlineData("a", "a", -10, ProductionLocationType.Bay, null, 5, null)]
    [InlineData("a", "a", -10, ProductionLocationType.Bay, ProductionLine.Gen2, null, null)]
    public void LocationCreationThrowsNull(
        string locationName, 
        string locationDescription, 
        decimal locationOrder, 
        ProductionLocationType type, 
        ProductionLine? productionLine = null, 
        int? bayNumber = null, 
        IEnumerable<string>? inventoryLocations = null)
    {
        Assert.Throws<ArgumentNullException>(() => TestLocations.LocationFactory.CreateLocation(locationName, locationDescription, locationOrder, type,
            productionLine, bayNumber, inventoryLocations));
    }
    
    [Theory]
    [InlineData("a", "a", 1, null)]
    public void LocationBayNumberAlreadyExists(
        string locationName, 
        string locationDescription, 
        int bayNumber, 
        IEnumerable<string>? inventoryLocations = null)
    {
        Assert.Throws<ArgumentException>(() => TestLocations.LocationFactory.CreateGen2BayLocation(locationName, locationDescription, bayNumber, inventoryLocations));
        
        Assert.Throws<ArgumentException>(() => TestLocations.LocationFactory.CreateExpoBayLocation(locationName, locationDescription, bayNumber, inventoryLocations));
    }
}