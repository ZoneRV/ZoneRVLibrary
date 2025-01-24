using System.ComponentModel.DataAnnotations;
using ZoneRV.Models.Production;
using ZoneRV.Models.Enums;
using ZoneRV.Models.Location;

namespace ZoneRV.Tests.Objects;

public static class ProductionTestData
{
    public static List<ProductionLine> ProductionLines => [Gen2, Expo];
    public static List<Model> VanModels => ProductionLines.SelectMany(x => x.Models).ToList();

    public static readonly ProductionLine Gen2;
    public static readonly ProductionLine Expo;

    public static Model Zsp;
    public static Model Zpp;
    public static Model Zss;
    public static Model Exp;
    
    public static LocationFactory LocationFactory;

    public static ZoneRV.Models.Location.Location G2Chassis;
    public static ZoneRV.Models.Location.Location ExpoChassis;
    
    public static ZoneRV.Models.Location.Location Subs;
    public static ZoneRV.Models.Location.Location Cabs;
    
    public static ZoneRV.Models.Location.Location G2WallRoof;
    public static ZoneRV.Models.Location.Location ExpoWallRoof;
    
    public static ZoneRV.Models.Location.Location G2Bay1;
    public static ZoneRV.Models.Location.Location G2Bay2;
    public static ZoneRV.Models.Location.Location G2Bay3;
    public static ZoneRV.Models.Location.Location G2Bay4;
    public static ZoneRV.Models.Location.Location G2Bay5;
    public static ZoneRV.Models.Location.Location G2Bay6;
    public static ZoneRV.Models.Location.Location G2Bay7;

    public static ZoneRV.Models.Location.Location ExpoBay1;
    public static ZoneRV.Models.Location.Location ExpoBay2;
    public static ZoneRV.Models.Location.Location ExpoBay3;
    public static ZoneRV.Models.Location.Location ExpoBay4;
    public static ZoneRV.Models.Location.Location ExpoBay5;

    static ProductionTestData()
    {
#region Production Lines and Models
        int expoId = IdUtils.ProductionLineId;
        Expo = new ProductionLine()
            {
                Id = expoId, 
                Name = "Expedition", 
                Models = []
            };

        Exp = new Model()
            {
                Id = IdUtils.ModelId,
                Name = "Expo",
                Description = "expo van",
                LineId = expoId,
                Prefix = "exp",
                ProductionLine = Expo
            };
        
        Expo.Models.Add(Exp);
        
        int gen2Id = IdUtils.ProductionLineId;
        Gen2 = new ProductionLine()
            {
                Id = IdUtils.ProductionLineId, 
                Name = "Gen 2", 
                Models = []
            };

        Zsp = new Model()
        {
            Id = IdUtils.ModelId,
            Name = "Sojourn",
            Description = "21\"",
            LineId = gen2Id,
            Prefix = "zsp",
            ProductionLine = Gen2
        };
        
        Zpp = new Model()
        {
            Id = IdUtils.ModelId,
            Name = "Peregrine",
            Description = "19\"",
            LineId = gen2Id,
            Prefix = "zpp",
            ProductionLine = Gen2
        };
        
        Zss = new Model()
        {
            Id = IdUtils.ModelId,
            Name = "Summit",
            Description = "The big boy",
            LineId = gen2Id,
            Prefix = "zss",
            ProductionLine = Gen2
        };
        
        Gen2.Models = [Zsp, Zpp, Zss];
#endregion

#region Line Locations

        
        
        G2Chassis = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Order               = 0,
            Line                = Gen2,
            Name                = "Gen2 chassis",
            Description         = "Chassis are built here",
            Type                = ProductionLocationType.Module,
            CustomLocationNames = [],
            InventoryLocations  = []
        };
        
        var g2ChassisCustomName = new LocationCustomName()
        {
            Id = IdUtils.LocationNameId, Location = G2Chassis, ServiceType = "test", CustomName = "chassis"
        };
        
        G2Chassis.CustomLocationNames.Add(g2ChassisCustomName);

        ExpoChassis = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Order               = 0,
            Line                = Expo,
            Name                = "Expo chassis",
            Description         = "Chassis are built here",
            Type                = ProductionLocationType.Module,
            CustomLocationNames = [],
            InventoryLocations  = []
        };

        var expoChassisCustomName = new LocationCustomName()
        {
            Id = IdUtils.LocationNameId, Location = ExpoChassis, ServiceType = "test", CustomName = "chassis"
        };
        ExpoChassis.CustomLocationNames.Add(expoChassisCustomName);

        Subs = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Order               = 0.25M,
            Name                = "Sub Assembly",
            Description         = "Stuffs are built here",
            Type                = ProductionLocationType.Subassembly,
            CustomLocationNames = [],
            InventoryLocations  = []
        };

        var subsCustomName = new LocationCustomName()
        {
            Id = IdUtils.LocationNameId, Location = Subs, ServiceType = "test", CustomName = "subs"
        };
        Subs.CustomLocationNames.Add(subsCustomName);

        Cabs = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Order               = 0.5M,
            Name                = "Cabinetry",
            Description         = "Cabs are built here",
            Type                = ProductionLocationType.Module,
            CustomLocationNames = [],
            InventoryLocations  = []
        };

        var cabsCustomName = new LocationCustomName()
        {
            Id = IdUtils.LocationNameId, Location = Cabs, ServiceType = "test", CustomName = "cabs"
        };
        Cabs.CustomLocationNames.Add(cabsCustomName);

        G2WallRoof = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Gen2,
            Order               = 3.5M,
            Name                = "Wall/Mod",
            Description         = "Walls are built here",
            Type                = ProductionLocationType.Module,
            CustomLocationNames = [],
            InventoryLocations  = []
        };

        var g2WallRoofCustomName = new LocationCustomName()
        {
            Id = IdUtils.LocationNameId, Location = G2WallRoof, ServiceType = "test", CustomName = "wallroof"
        };
        G2WallRoof.CustomLocationNames.Add(g2WallRoofCustomName);

        ExpoWallRoof = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Expo,
            Order               = 2.5M,
            Name                = "Wall/Mod",
            Description         = "Walls are built here",
            Type                = ProductionLocationType.Module,
            CustomLocationNames = [],
            InventoryLocations  = []
        };

        var expoWallRoofCustomName = new LocationCustomName()
        {
            Id = IdUtils.LocationNameId, Location = ExpoWallRoof, ServiceType = "test", CustomName = "wallroof"
        };
        ExpoWallRoof.CustomLocationNames.Add(expoWallRoofCustomName);

        G2Bay1 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Gen2,
            Order               = 1M,
            BayNumber           = 1,
            Name                = "Bay 1",
            Description         = "Bay 1 of Gen 2",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [],
            InventoryLocations  = []
        };
        
        var g2Bay1CustomName = new LocationCustomName()
        {
            Id = IdUtils.LocationNameId, Location = G2Bay1, ServiceType = "test", CustomName = "bay1"
        };
        G2Bay1.CustomLocationNames.Add(g2Bay1CustomName);

        G2Bay2 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Gen2,
            Order               = 2M,
            BayNumber           = 2,
            Name                = "Bay 2",
            Description         = "Bay 2 of Gen 2",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [],
            InventoryLocations  = []
        };
        
        var g2Bay2CustomName = new LocationCustomName()
        {
            Id = IdUtils.LocationNameId, Location = G2Bay2, ServiceType = "test", CustomName = "bay2"
        };
        G2Bay2.CustomLocationNames.Add(g2Bay2CustomName);

        G2Bay3 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Gen2,
            Order               = 3M,
            BayNumber           = 3,
            Name                = "Bay 3",
            Description         = "Bay 3 of Gen 2",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [],
            InventoryLocations  = []
        };
        
        var g2Bay3CustomName = new LocationCustomName()
        {
            Id = IdUtils.LocationNameId, Location = G2Bay3, ServiceType = "test", CustomName = "bay3"
        };
        G2Bay3.CustomLocationNames.Add(g2Bay3CustomName);

        G2Bay4 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Gen2,
            Order               = 4M,
            BayNumber           = 4,
            Name                = "Bay 4",
            Description         = "Bay 4 of Gen 2",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [],
            InventoryLocations  = []
        };
        
        var g2Bay4CustomName = new LocationCustomName()
        {
            Id = IdUtils.LocationNameId, Location = G2Bay4, ServiceType = "test", CustomName = "bay4"
        };
        G2Bay4.CustomLocationNames.Add(g2Bay4CustomName);

        G2Bay5 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Gen2,
            Order               = 5M,
            BayNumber           = 5,
            Name                = "Bay 5",
            Description         = "Bay 5 of Gen 2",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [],
            InventoryLocations  = []
        };
        
        var g2Bay5CustomName = new LocationCustomName()
        {
            Id = IdUtils.LocationNameId, Location = G2Bay5, ServiceType = "test", CustomName = "bay5"
        };
        G2Bay5.CustomLocationNames.Add(g2Bay5CustomName);

        G2Bay6 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Gen2,
            Order               = 6M,
            BayNumber           = 6,
            Name                = "Bay 6",
            Description         = "Bay 6 of Gen 2",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [],
            InventoryLocations  = []
        };
        
        var g2Bay6CustomName = new LocationCustomName()
        {
            Id = IdUtils.LocationNameId, Location = G2Bay6, ServiceType = "test", CustomName = "bay6"
        };
        G2Bay6.CustomLocationNames.Add(g2Bay6CustomName);

        G2Bay7 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Gen2,
            Order               = 7M,
            BayNumber           = 7,
            Name                = "Bay 7",
            Description         = "Bay 7 of Gen 2",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [],
            InventoryLocations  = []
        };
        
        var g2Bay7CustomName = new LocationCustomName()
        {
            Id = IdUtils.LocationNameId, Location = G2Bay7, ServiceType = "test", CustomName = "bay7"
        };
        G2Bay7.CustomLocationNames.Add(g2Bay7CustomName);

        ExpoBay1 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Expo,
            Order               = 1M,
            BayNumber           = 1,
            Name                = "Bay 1",
            Description         = "Bay 1 of Expo",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [],
            InventoryLocations  = []
        };
        
        var expoBay1CustomName = new LocationCustomName()
        {
            Id = IdUtils.LocationNameId, Location = ExpoBay1, ServiceType = "test", CustomName = "bay1"
        };
        ExpoBay1.CustomLocationNames.Add(expoBay1CustomName);

        ExpoBay2 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Expo,
            Order               = 2M,
            BayNumber           = 2,
            Name                = "Bay 2",
            Description         = "Bay 2 of Expo",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [],
            InventoryLocations  = []
        };
        
        var expoBay2CustomName = new LocationCustomName()
        {
            Id = IdUtils.LocationNameId, Location = ExpoBay2, ServiceType = "test", CustomName = "bay2"
        };
        ExpoBay2.CustomLocationNames.Add(expoBay2CustomName);

        ExpoBay3 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Expo,
            Order               = 3M,
            BayNumber           = 3,
            Name                = "Bay 3",
            Description         = "Bay 3 of Expo",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [],
            InventoryLocations  = []
        };
        
        var expoBay3CustomName = new LocationCustomName()
        {
            Id = IdUtils.LocationNameId, Location = ExpoBay3, ServiceType = "test", CustomName = "bay3"
        };
        ExpoBay3.CustomLocationNames.Add(expoBay3CustomName);

        ExpoBay4 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Expo,
            Order               = 4M,
            BayNumber           = 4,
            Name                = "Bay 4",
            Description         = "Bay 4 of Expo",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [],
            InventoryLocations  = []
        };
        
        var expoBay4CustomName = new LocationCustomName()
        {
            Id = IdUtils.LocationNameId, Location = ExpoBay4, ServiceType = "test", CustomName = "bay4"
        };
        ExpoBay4.CustomLocationNames.Add(expoBay4CustomName);

        ExpoBay5 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Expo,
            Order               = 5M,
            BayNumber           = 5,
            Name                = "Bay 5",
            Description         = "Bay 5 of Expo",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [],
            InventoryLocations  = []
        };
        
        var expoBay5CustomName = new LocationCustomName()
        {
            Id = IdUtils.LocationNameId, Location = ExpoBay5, ServiceType = "test", CustomName = "bay5"
        };
        ExpoBay5.CustomLocationNames.Add(expoBay5CustomName);
        
        LocationFactory = new LocationFactory()
        {
            IgnoredListNames = ["ignored"],
            Locations = new LocationCollection
            ([
                ExpoChassis,
                G2Chassis,
                G2WallRoof,
                ExpoWallRoof,
                Subs,
                Cabs,
                G2Bay1,
                G2Bay2,
                G2Bay3,
                G2Bay4,
                G2Bay5,
                G2Bay6,
                G2Bay7,
                ExpoBay1,
                ExpoBay2,
                ExpoBay3,
                ExpoBay4,
                ExpoBay5
            ])
        };
#endregion
    }

    private static readonly DateTimeOffset CurrentTime = DateTimeOffset.Now;

    /// <param name="percentage">Value from 0.0 to 1.0</param>
    public static LocationInfo GetLocationInfo(ProductionLine line, [Range(0f, 1f)] float percentage)
    {
        LocationInfo results = new LocationInfo();
        
        var positions = LocationFactory.Locations.Where(x 
            =>  x.Line is not null && 
                x.Line == line && 
                x.Type == ProductionLocationType.Bay).ToList();
        
        positions.AddRange([LocationFactory.PreProduction, LocationFactory.PostProduction]);
        positions = positions.OrderByDescending(x => x.Order).ToList();

        for (int i = 0; i < (float)positions.Count * percentage; i++)
        {
            results.AddPositionChange(CurrentTime - TimeSpan.FromDays(i), positions.ElementAt(i));
        }

        return results;
    }
}