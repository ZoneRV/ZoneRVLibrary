using Microsoft.Extensions.Configuration;
using ZoneRV.Models;
using ZoneRV.Models.Enums;
using ZoneRV.Models.Location;
using ZoneRV.Services.Production;
using ZoneRV.Tests.Objects;

namespace ZoneRV.Tests.Models;

public class TestProductionService : IProductionService
{
    private readonly int _boardCount;
    private readonly int _boardsHandedover;
    private readonly int _boardsHandoverOverDue;
    private readonly int _boardsInCarPark;

    private readonly DateTimeOffset _startTime = DateTimeOffset.Now;

    private readonly TimeOnly _firstLineMove;
    private readonly TimeOnly _secondLineMove;
    
    public TimeSpan LoadTimeForBoard { get; set; }

    public override int MaxDegreeOfParallelism { get; protected set; } = 5;

    public TestProductionService(IConfiguration configuration, IEnumerable<ProductionLine> productionLines, int boardCount, int boardsHandedover, int boardsHandoverOverDue, int boardsInCarPark, TimeOnly firstLineMove, TimeOnly secondLineMove, TimeSpan? loadTimeForBoard = null) : base(configuration, productionLines)
    {
        LoadTimeForBoard = loadTimeForBoard ?? TimeSpan.Zero;

        _boardCount = boardCount;
        _boardsHandedover = boardsHandedover;
        _boardsHandoverOverDue = boardsHandoverOverDue;
        _boardsInCarPark = boardsInCarPark;

        _firstLineMove = firstLineMove;
        _secondLineMove = secondLineMove;
    }

    public sealed override LocationFactory LocationFactory { get => ProductionTestData.LocationFactory; init => _ = value; }
    
    public override Task InitialiseService()
    {
        var models = ProductionLines.SelectMany(x => x.Models).ToList();
        
        for (int i = 0; i < _boardCount + 1; i++)
        {
            var vanModel = models.ElementAt(i % models.Count);
            var vanName = vanModel.Prefix + (i + 1).ToString("000");

            var locationInfo = new VanLocationInfo();
            
            var van = new VanProductionInfo()
            {
                Name = vanName,
                VanModel = vanModel,
                Url = $"https://www.google.com/search?q=fun+facts+about+the+number+{i}",
                Id = i.ToString(),
                LocationInfo = locationInfo
            };
            
            DateTimeOffset handover = _startTime.LocalDateTime.Date - TimeSpan.FromDays((_boardsHandedover + _boardsHandoverOverDue - i) / 2) + (i % 2 == 0 ? _firstLineMove.ToTimeSpan() : _secondLineMove.ToTimeSpan());
            DateTimeOffset? handoverStateUpdated = null;
            HandoverState handoverState = HandoverState.UnhandedOver;
            
            van.AddHandoverHistory(handover - TimeSpan.FromDays(30), handover);
            
            if (i < _boardsHandedover)
            {
                handoverState = HandoverState.HandedOver;
                handoverStateUpdated = handover + TimeSpan.FromHours(1);
            }
                
            if (i % 3 == 0)
            {
                van.AddHandoverHistory(_startTime - TimeSpan.FromDays(2), handover + TimeSpan.FromMinutes(15));
            }

            van.HandoverState = handoverState;
            van.HandoverStateLastUpdated = handoverStateUpdated;
            
            van.LocationInfo.AddPositionChange(handover - TimeSpan.FromDays(60), LocationFactory.PreProduction);
            var allLocations = LocationFactory.GetAllLocationsFromLine(vanModel.ProductionLine).Where(x => x.Type == ProductionLocationType.Bay).ToList();

            var locations = allLocations.Take(_boardsInCarPark + _boardsHandedover + _boardsHandoverOverDue - 2 - i).ToList();
            
            if(locations.Any())
            {
                int moves = Math.Abs(_boardsInCarPark + _boardsHandedover + _boardsHandoverOverDue - i);
                
                if (allLocations.Count == locations.Count)
                    van.LocationInfo.AddPositionChange(_startTime.LocalDateTime.Date - TimeSpan.FromDays(moves / 2) + (moves % 2 == 1 ? _firstLineMove.ToTimeSpan() : _secondLineMove.ToTimeSpan()), LocationFactory.PostProduction);

                foreach (var location in locations.OrderByDescending(x => x.Order))
                {
                    moves++;
                    van.LocationInfo.AddPositionChange(_startTime.LocalDateTime.Date - TimeSpan.FromDays(moves / 2) + (moves % 2 == 1 ? _firstLineMove.ToTimeSpan() : _secondLineMove.ToTimeSpan()), location);
                }
            }
            
            Vans.TryAdd(vanName, van);
        }
        
        return Task.CompletedTask;
    }

    protected override async Task<VanProductionInfo> _loadVanFromSourceAsync(VanProductionInfo info)
    {
        await Task.Delay(LoadTimeForBoard);
        throw new NotImplementedException();
    }
}