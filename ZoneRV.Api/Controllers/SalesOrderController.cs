using Microsoft.AspNetCore.Mvc;
using ZoneRV.Serialization;
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
    public async Task<ActionResult<SalesOrder>> Get(string name, bool loadProduction, [FromBody] List<string> includedFields)
    {
        if (!ProductionService.TryGetInfoByName(name, out var info))
            return NotFound();

        if (loadProduction && !info.ProductionInfoLoaded)
            await ProductionService.LoadVanBoardAsync(info);
        
        var json = JsonConvert.SerializeObject(
                            info, 
                            ZoneJsonSerializerSettings.GetOptionalSerializerSettings(includedFields)
                            );
        
        return Ok(json);
    }
}