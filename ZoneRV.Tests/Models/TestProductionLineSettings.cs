using ZoneRV.Models;

namespace ZoneRV.Tests.Models;

public class TestProductionLineSettings(ProductionLine productionLine, TimeOnly[] moveTimes, int boardCount, int boardsHandedOver, int boardsHandoverOverDue, int boardsInCarPark)
{
    public ProductionLine ProductionLine        { get; set; } = productionLine;
    public TimeOnly[]     MoveTimes             { get; set; } = moveTimes;
    public int            BoardCount            { get; set; } = boardCount;
    public int            BoardsHandedOver      { get; set; } = boardsHandedOver;
    public int            BoardsHandoverOverDue { get; set; } = boardsHandoverOverDue;
    public int            BoardsInCarPark       { get; set; } = boardsInCarPark;
}