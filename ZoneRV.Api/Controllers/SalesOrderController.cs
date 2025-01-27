using Microsoft.AspNetCore.Mvc;
using ZoneRV.Services.Production;

namespace ZoneRV.Api.Controllers;

[Route("api/[controller]"), ApiController]
public class SalesOrderController : ControllerBase
{
    private IProductionService ProductionService { get; set; }
    
    public SalesOrderController(IProductionService productionService)
    {
        ProductionService = productionService;
    }
    
    [HttpGet("{name}/{loadProduction}")]
    public async Task<ActionResult<SalesProductionInfo>> Get(string name, bool loadProduction)
    {
        if (!ProductionService.TryGetInfoByName(name, out var info))
            return NotFound();

        if (loadProduction && !info.ProductionInfoLoaded)
            await ProductionService.LoadVanBoardAsync(info);

        return Ok(info);
    }
}