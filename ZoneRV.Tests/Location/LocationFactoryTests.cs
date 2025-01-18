using ZoneRV.Models.Enums;
using ZoneRV.Models.Location;
using ZoneRV.Tests.Objects;

namespace ZoneRV.Tests.Location;

public class LocationFactoryTests
{
    [Theory]
    [InlineData("a", "a", -10, ProductionLocationType.Bay, null, 5, null)]
    [InlineData("a", "a", -10, ProductionLocationType.Bay, true, null, null)]
    public void LocationCreationThrowsNull(
        string locationName, 
        string locationDescription, 
        decimal locationOrder, 
        ProductionLocationType type, 
        bool? isGen2 = null, 
        int? bayNumber = null, 
        IEnumerable<string>? inventoryLocations = null)
    {
        Assert.Throws<ArgumentNullException>(() 
            => ProductionTestData.LocationFactory.CreateLocation(
                locationName,
                locationDescription, 
                locationOrder, 
                type,
                isGen2.HasValue ? 
                    (isGen2.Value ? ProductionTestData.Gen2 : ProductionTestData.Expo) : 
                    null, 
                bayNumber,
                inventoryLocations));
    }
    
    [Theory]
    [InlineData("a", "a", 1, null)]
    public void LocationBayNumberAlreadyExists(
        string locationName, 
        string locationDescription, 
        int bayNumber, 
        IEnumerable<string>? inventoryLocations = null)
    {
        Assert.Throws<ArgumentException>(() => ProductionTestData.LocationFactory.CreateBayLocation(locationName, locationDescription, ProductionTestData.Gen2, bayNumber, inventoryLocations));
        
        Assert.Throws<ArgumentException>(() => ProductionTestData.LocationFactory.CreateBayLocation(locationName, locationDescription, ProductionTestData.Gen2, bayNumber, inventoryLocations));
    }
}