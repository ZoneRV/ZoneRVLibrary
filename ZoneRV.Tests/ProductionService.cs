using Microsoft.Extensions.Configuration;
using ZoneRV.Services.Production;
using ZoneRV.Tests.Models;
using ZoneRV.Tests.Objects;

namespace ZoneRV.Tests;

public class ProductionService
{
    private IProductionService _productionService;
    
    private readonly int _boardCount = 100;
    private readonly int _boardsHandedover = 10;
    private readonly int _boardsHandoverOverDue = 10;
    private readonly int _boardsInCarPark = 20;
    
    private readonly TimeOnly _firstLineMove = new TimeOnly(10, 0);
    private readonly TimeOnly _secondLineMove = new TimeOnly(13, 0);
    
    public ProductionService()
    {
        IConfiguration config = new ConfigurationBuilder().Build();
        
        _productionService = new TestProductionService(config, ProductionTestData.ProductionLines, _boardCount, _boardsHandedover, _boardsHandoverOverDue, _boardsInCarPark, _firstLineMove, _secondLineMove, TimeSpan.FromSeconds(5));

        Task.FromResult(_productionService.InitialiseService());
    }

    [Fact]
    public void CarParkCount()
    {
        var actual = _productionService.VansInCarPark;
        
        if (DateTimeOffset.Now.LocalDateTime.TimeOfDay > _secondLineMove.ToTimeSpan())
            Assert.Equal(_boardsInCarPark + 2, actual);
        else if (DateTimeOffset.Now.LocalDateTime.TimeOfDay > _firstLineMove.ToTimeSpan())
            Assert.Equal(_boardsInCarPark + 1, actual);
        else
            Assert.Equal(_boardsInCarPark, actual);
    }

    [Fact]
    public void HandedOverCount()
    {
        var actual = _productionService.VansHandedOver;
        Assert.Equal(_boardsHandedover, actual);
    }

    [Fact]
    public void HandoverOverDueCount()
    {
        var actual = _productionService.VansHandoverOverdue;
        
        //Accounting for the current time of day in test
        
        if (DateTimeOffset.Now.LocalDateTime.TimeOfDay > _secondLineMove.ToTimeSpan())
            Assert.Equal(_boardsHandoverOverDue + 2, actual);
        else if (DateTimeOffset.Now.LocalDateTime.TimeOfDay > _firstLineMove.ToTimeSpan())
            Assert.Equal(_boardsHandoverOverDue + 1, actual);
        else
            Assert.Equal(_boardsHandoverOverDue, actual);
    }
}