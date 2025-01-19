using Microsoft.Extensions.Configuration;
using ZoneRV.Services.Production;
using ZoneRV.Tests.Models;
using ZoneRV.Tests.Objects;

namespace ZoneRV.Tests;

public class ProductionService
{
    private IProductionService               _productionService;
    private List<TestProductionLineSettings> _settings;
    
    public ProductionService()
    {
        IConfiguration config = new ConfigurationBuilder().Build();

        _settings =
        [
            new TestProductionLineSettings(
                ProductionTestData.Gen2, 
                [
                    new TimeOnly(10, 0), 
                    new TimeOnly(13, 0)
                ], 
                100, 
                10, 
                10, 
                20),
            new TestProductionLineSettings(
                ProductionTestData.Expo, 
                [
                    new TimeOnly(12,0)
                ], 
                50, 
                5, 
                5, 
                20)
        ];
        
        _productionService = new TestProductionService(config, _settings, TimeSpan.FromSeconds(5));

        Task.FromResult(_productionService.InitialiseService());
    }

    [Fact]
    public void CarParkCount()
    {
        var actual = _productionService.VansInCarPark;
        
        Assert.Equal(
            _settings.Sum(x => x.BoardsInCarPark) + 
            _settings.Sum(x => x.BoardsHandoverOverDue) + 
            _settings.Sum(x => x.MoveTimes.Count(y => y.ToTimeSpan() < DateTime.Now.TimeOfDay)), 
            actual);
    }

    [Fact]
    public void HandedOverCount()
    {
        var actual = _productionService.VansHandedOver;
        Assert.Equal(_settings.Sum(x => x.BoardsHandedOver), actual);
    }

    [Fact]
    public void HandoverOverDueCount()
    {
        var actual = _productionService.VansHandoverOverdue;
        
        Assert.Equal(
            _settings.Sum(x => x.BoardsHandoverOverDue) + 
            _settings.Sum(x => x.MoveTimes.Count(y => y.ToTimeSpan() < DateTime.Now.TimeOfDay)), 
            actual);
    }
}