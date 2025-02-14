﻿using ZoneRV.Models.Enums;
using ZoneRV.Models.Location;

namespace ZoneRV.Tests.Location;

public class LocationInfoTests
{
    /*LocationInfo fullHistory => ProductionTestData.GetLocationInfo(ProductionTestData.Gen2, 1);
    LocationInfo halfHistory => ProductionTestData.GetLocationInfo(ProductionTestData.Gen2, .5f);

    [Fact]
    public void CannotAddLocationThatExistAlready()
    {
        // Location Exists
        Assert.Throws<ArgumentException>(
            "location",
            () => fullHistory.AddPositionChange(DateTimeOffset.MaxValue, LocationFactory.PreProduction));
    }

    [Fact]
    public void CannotNonBayLocation()
    {
        // Cant add Non bays (unless pre or post-production)
        Assert.Throws<ArgumentException>(
            "Type",
            () =>fullHistory.AddPositionChange(DateTimeOffset.MaxValue, ProductionTestData.LocationFactory.Locations.First(x => x.Type == ProductionLocationType.Module)));
    }

    [Fact]
    public void CannotBayFromDifferentLine()
    {
        // Cant add different production lines
        Assert.Throws<ArgumentException>(
            "Line",
            () => fullHistory.AddPositionChange(DateTimeOffset.MaxValue,
                ProductionTestData.LocationFactory.Locations.First(x => x.Line == ProductionTestData.Expo && x.Type == ProductionLocationType.Bay)));
    }
    
    [Fact]
    public void CannotAddMoveWithExistingDate()
    {
        // Cant add move with already existing date
        Assert.Throws<ArgumentException>(
            "date",
            () => halfHistory.AddPositionChange(halfHistory.MinBy(x => x.moveDate).moveDate,
                new ZoneRV.Models.Location.Location(){BayNumber = 8, Order = 8, Name = "fail", Description = "made to fail", Type = ProductionLocationType.Bay, Line = ProductionTestData.Gen2}));
    }*/
}