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
            CustomLocationNames = [new LocationCustomName(){ Id = IdUtils.LocationNameId, Line = Gen2, ServiceType = "test", CustomName = "chassis"}],
            InventoryLocations  = []
        };

        ExpoChassis = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Order               = 0,
            Line                = Expo,
            Name                = "Expo chassis",
            Description         = "Chassis are built here",
            Type                = ProductionLocationType.Module,
            CustomLocationNames = [new LocationCustomName(){ Id = IdUtils.LocationNameId, Line = Expo, ServiceType = "test", CustomName = "chassis"}],
            InventoryLocations  = []
        };

        Subs = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Order               = 0.25M,
            Name                = "Sub Assembly",
            Description         = "Stuffs are built here",
            Type                = ProductionLocationType.Subassembly,
            CustomLocationNames = [new LocationCustomName(){ Id = IdUtils.LocationNameId, Line = Gen2, ServiceType = "test", CustomName = "subs"}],
            InventoryLocations  = []
        };

        Cabs = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Order               = 0.5M,
            Name                = "Cabinetry",
            Description         = "Cabs are built here",
            Type                = ProductionLocationType.Module,
            CustomLocationNames = [new LocationCustomName(){ Id = IdUtils.LocationNameId, Line = Gen2, ServiceType = "test", CustomName = "cabs"}],
            InventoryLocations  = []
        };

        G2WallRoof = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Gen2,
            Order               = 3.5M,
            Name                = "Wall/Mod",
            Description         = "Walls are built here",
            Type                = ProductionLocationType.Module,
            CustomLocationNames = [new LocationCustomName(){ Id = IdUtils.LocationNameId, Line = Gen2, ServiceType = "test", CustomName = "wallroof"}],
            InventoryLocations  = []
        };

        ExpoWallRoof = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Expo,
            Order               = 2.5M,
            Name                = "Wall/Mod",
            Description         = "Walls are built here",
            Type                = ProductionLocationType.Module,
            CustomLocationNames = [new LocationCustomName(){ Id = IdUtils.LocationNameId, Line = Expo, ServiceType = "test", CustomName = "wallroof"}],
            InventoryLocations  = []
        };

        G2Bay1 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Gen2,
            Order               = 1M,
            BayNumber           = 1,
            Name                = "Bay 1",
            Description         = "Bay 1 of Gen 2",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [new LocationCustomName(){ Id = IdUtils.LocationNameId, Line = Gen2, ServiceType = "test", CustomName = "bay1"}],
            InventoryLocations  = []
        };

        G2Bay2 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Gen2,
            Order               = 2M,
            BayNumber           = 2,
            Name                = "Bay 2",
            Description         = "Bay 2 of Gen 2",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [new LocationCustomName(){ Id = IdUtils.LocationNameId, Line = Gen2, ServiceType = "test", CustomName = "bay2"}],
            InventoryLocations  = []
        };

        G2Bay3 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Gen2,
            Order               = 3M,
            BayNumber           = 3,
            Name                = "Bay 3",
            Description         = "Bay 3 of Gen 2",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [new LocationCustomName(){ Id = IdUtils.LocationNameId, Line = Gen2, ServiceType = "test", CustomName = "bay3"}],
            InventoryLocations  = []
        };

        G2Bay4 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Gen2,
            Order               = 4M,
            BayNumber           = 4,
            Name                = "Bay 4",
            Description         = "Bay 4 of Gen 2",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [new LocationCustomName(){ Id = IdUtils.LocationNameId, Line = Gen2, ServiceType = "test", CustomName = "bay4"}],
            InventoryLocations  = []
        };

        G2Bay5 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Gen2,
            Order               = 5M,
            BayNumber           = 5,
            Name                = "Bay 5",
            Description         = "Bay 5 of Gen 2",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [new LocationCustomName(){ Id = IdUtils.LocationNameId, Line = Gen2, ServiceType = "test", CustomName = "bay5"}],
            InventoryLocations  = []
        };

        G2Bay6 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Gen2,
            Order               = 6M,
            BayNumber           = 6,
            Name                = "Bay 6",
            Description         = "Bay 6 of Gen 2",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [new LocationCustomName(){ Id = IdUtils.LocationNameId, Line = Gen2, ServiceType = "test", CustomName = "bay6"}],
            InventoryLocations  = []
        };

        G2Bay7 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Gen2,
            Order               = 7M,
            BayNumber           = 7,
            Name                = "Bay 7",
            Description         = "Bay 7 of Gen 2",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [new LocationCustomName(){ Id = IdUtils.LocationNameId, Line = Gen2, ServiceType = "test", CustomName = "bay7"}],
            InventoryLocations  = []
        };

        ExpoBay1 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Expo,
            Order               = 1M,
            BayNumber           = 1,
            Name                = "Bay 1",
            Description         = "Bay 1 of Expo",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [new LocationCustomName(){ Id = IdUtils.LocationNameId, Line = Expo, ServiceType = "test", CustomName = "bay1"}],
            InventoryLocations                                  = []
        };

        ExpoBay2 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Expo,
            Order               = 2M,
            BayNumber           = 2,
            Name                = "Bay 2",
            Description         = "Bay 2 of Expo",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [new LocationCustomName(){ Id = IdUtils.LocationNameId, Line = Expo, ServiceType = "test", CustomName = "bay2"}],
            InventoryLocations                                  = []
        };

        ExpoBay3 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Expo,
            Order               = 3M,
            BayNumber           = 3,
            Name                = "Bay 3",
            Description         = "Bay 3 of Expo",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [new LocationCustomName(){ Id = IdUtils.LocationNameId, Line = Expo, ServiceType = "test", CustomName = "bay3"}],
            InventoryLocations                                  = []
        };

        ExpoBay4 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Expo,
            Order               = 4M,
            BayNumber           = 4,
            Name                = "Bay 4",
            Description         = "Bay 4 of Expo",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [new LocationCustomName(){ Id = IdUtils.LocationNameId, Line = Expo, ServiceType = "test", CustomName = "bay4"}],
            InventoryLocations                                  = []
        };

        ExpoBay5 = new ZoneRV.Models.Location.Location()
        {
            Id                  = IdUtils.LocationId,
            Line                = Expo,
            Order               = 5M,
            BayNumber           = 5,
            Name                = "Bay 5",
            Description         = "Bay 5 of Expo",
            Type                = ProductionLocationType.Bay,
            CustomLocationNames = [new LocationCustomName(){ Id = IdUtils.LocationNameId, Line = Expo, ServiceType = "test", CustomName = "bay5"}],
            InventoryLocations                                  = []
        };
        
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
    
    static readonly DateTimeOffset currentTime = DateTimeOffset.Now;

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
            results.AddPositionChange(currentTime - TimeSpan.FromDays(i), positions.ElementAt(i));
        }

        return results;
    }
}