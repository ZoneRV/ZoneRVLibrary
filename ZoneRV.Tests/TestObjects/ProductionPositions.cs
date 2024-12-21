using ZoneRV.Models.Enums;
using ZoneRV.Models.ProductionPosition;

namespace ZoneRV.Tests.TestObjects;

public static class ProductionPositions
{
    
    public static ProductionPosition g1Pos = new ProductionPosition
    {
        Type = ProductionPositionType.Bay,
        ProductionLine = ProductionLine.Gen2,
        PositionName = "Bay 1",
        PositionDescription = "Bay 1 of gen 2",
        PositionOrder = 1,
        InventoryLocations = []
    };
    
    public static ProductionPosition g1PosIdentical = new ProductionPosition
    {
        Type = ProductionPositionType.Bay,
        ProductionLine = ProductionLine.Gen2,
        PositionName = "Bay 1",
        PositionDescription = "Bay 1 of gen 2",
        PositionOrder = 1,
        InventoryLocations = []
    };
    
    public static ProductionPosition g2Pos = new ProductionPosition
    {
        Type = ProductionPositionType.Bay,
        ProductionLine = ProductionLine.Gen2,
        PositionName = "Bay 2",
        PositionDescription = "Bay 2 of gen 2",
        PositionOrder = 2,
        InventoryLocations = []
    };
    
    public static ProductionPosition g3Pos = new ProductionPosition
    {
        Type = ProductionPositionType.Bay,
        ProductionLine = ProductionLine.Gen2,
        PositionName = "Bay 3",
        PositionDescription = "Bay 3 of gen 2",
        PositionOrder = 3.1M,
        InventoryLocations = []
    };
    
    public static ProductionPosition g4Pos = new ProductionPosition
    {
        Type = ProductionPositionType.Bay,
        ProductionLine = ProductionLine.Gen2,
        PositionName = "Bay 4",
        PositionDescription = "Bay 4 of gen 2",
        PositionOrder = 4,
        InventoryLocations = []
    };

}