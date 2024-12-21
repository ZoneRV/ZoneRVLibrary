using ZoneRV.Models.ProductionPosition;

namespace ZoneRV.Tests.TestObjects;

public static class PositionInfos
{
    static PositionInfos()
    {
        FullGen2Info.AddPositionChange(DateTimeOffset.Now - TimeSpan.FromDays(5), ProductionPosition.PreProduction);
        FullGen2Info.AddPositionChange(DateTimeOffset.Now - TimeSpan.FromDays(4), ProductionPositions.g1Pos);
        FullGen2Info.AddPositionChange(DateTimeOffset.Now - TimeSpan.FromDays(3), ProductionPositions.g2Pos);
        FullGen2Info.AddPositionChange(DateTimeOffset.Now - TimeSpan.FromDays(2), ProductionPositions.g3Pos);
        FullGen2Info.AddPositionChange(DateTimeOffset.Now - TimeSpan.FromDays(1), ProductionPositions.g4Pos);
        FullGen2Info.AddPositionChange(DateTimeOffset.Now - TimeSpan.FromHours(1), ProductionPosition.PostProduction);
        
        HalfGen2Info.AddPositionChange(DateTimeOffset.Now - TimeSpan.FromDays(3), ProductionPosition.PreProduction);
        HalfGen2Info.AddPositionChange(DateTimeOffset.Now - TimeSpan.FromDays(2), ProductionPositions.g1Pos);
        HalfGen2Info.AddPositionChange(DateTimeOffset.Now - TimeSpan.FromDays(1), ProductionPositions.g2Pos);
    }
    
    
    public static PositionInfo FullGen2Info = new ();
    public static PositionInfo HalfGen2Info = new ();
    public static PositionInfo EmptyInfo = new ();
}