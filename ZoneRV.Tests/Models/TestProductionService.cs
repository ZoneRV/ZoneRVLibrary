using Microsoft.Extensions.Configuration;
using ZoneRV.Models;
using ZoneRV.Models.Enums;
using ZoneRV.Models.Location;
using ZoneRV.Services.Production;
using ZoneRV.Tests.Objects;

namespace ZoneRV.Tests.Models;

public class TestProductionService : IProductionService
{
    private readonly DateTimeOffset _startTime = DateTimeOffset.Now;

    public List<TestProductionLineSettings> LineSettings { get; set; }
    
    public             TimeSpan LoadTimeForBoard       { get; set; }
    public override    int      MaxDegreeOfParallelism { get; protected set; } = 5;
    protected override string   LocationTypeName       { get => "test"; }

    public TestProductionService(IConfiguration configuration, List<TestProductionLineSettings> lineSettings, TimeSpan? loadTimeForBoard = null) : base(configuration, lineSettings.Select(x => x.ProductionLine))
    {
        LoadTimeForBoard = loadTimeForBoard ?? TimeSpan.Zero;

        LineSettings = lineSettings.ToList();
    }

    public sealed override LocationFactory LocationFactory { get => ProductionTestData.LocationFactory; init => _ = value; }
    
    
    public override Task InitialiseService()
    {
        foreach (var line in LineSettings)
        {
            for (int i = 0; i < line.BoardCount; i++)
            {
                var model = line.ProductionLine.Models.ElementAt(i % line.ProductionLine.Models.Count);
                var name  = model.Prefix.ToUpper() + (i + 1).ToString("000");
                
                var locationInfo = new VanLocationInfo();
            
                var van = new VanProductionInfo()
                {
                    Name = name,
                    VanModel = model,
                    Url = $"https://www.google.com/search?q=fun+facts+about+the+number+{i}",
                    Id = name.GetHashCode().ToString(),
                    LocationInfo = locationInfo
                };
                
                DateTimeOffset  handover             = _startTime.LocalDateTime.Date - TimeSpan.FromDays((line.BoardsHandedOver + line.BoardsHandoverOverDue - i) / line.MoveTimes.Length) + line.MoveTimes[i % line.MoveTimes.Length].ToTimeSpan();
                DateTimeOffset? handoverStateUpdated = null;
                HandoverState   handoverState        = HandoverState.UnhandedOver;
                
                van.AddHandoverHistory(handover - TimeSpan.FromDays(30), handover);
                
                if (i < line.BoardsHandedOver)
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
                var allLocations = LocationFactory.GetAllLocationsFromLine(model.ProductionLine).Where(x => x.Type == ProductionLocationType.Bay).ToList();

                List<ProductionLocation> locations;
                int moves = line.BoardsInCarPark + line.BoardsHandedOver + line.BoardsHandoverOverDue - i;

                if (moves > 0)
                    locations = allLocations.ToList();
                else
                    locations = allLocations.Skip(-moves).ToList();
                
                if(locations.Any())
                {
                    if (moves > 0)
                        van.LocationInfo.AddPositionChange(_startTime.LocalDateTime.Date + TimeSpan.FromDays(moves / line.MoveTimes.Length) + line.MoveTimes[Math.Abs(moves) % line.MoveTimes.Length].ToTimeSpan(), LocationFactory.PostProduction);

                    foreach (var location in locations.OrderByDescending(x => x.Order))
                    {
                        if (moves == 0) // Account for zero and -2 both adding positions on same time
                            moves--;
                        
                        moves--;
                        van.LocationInfo.AddPositionChange(_startTime.LocalDateTime.Date + TimeSpan.FromDays(moves / line.MoveTimes.Length) + line.MoveTimes[Math.Abs(moves) % line.MoveTimes.Length].ToTimeSpan(), location);
                    }
                }
                
                Vans.TryAdd(name.ToLower(), van);
            }
        }
        
        //TODO: Assert some basic facts to make sure everything is fine
        
        return Task.CompletedTask;
    }

    protected override async Task<VanProductionInfo> _loadVanFromSourceAsync(VanProductionInfo info)
    {
        await Task.Delay(LoadTimeForBoard);
        throw new NotImplementedException();
    }
}