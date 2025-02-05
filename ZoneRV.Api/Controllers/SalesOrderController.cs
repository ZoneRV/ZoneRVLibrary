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
    
    [HttpGet("{name}")]
    public async Task<ActionResult<SalesOrder>> Get(string name, [FromBody] List<string>? includedFields = null)
    {
        if (!ProductionService.TryGetInfoByName(name, out var info))
            return NotFound();

        includedFields = includedFields ?? [];

        bool productionReloadNeeded = false;
        
        foreach (var field in includedFields)
        {
            SerializationUtils.CheckForRequired(field, out var prod, out _);

            if (prod)
            {
                productionReloadNeeded = true;
                break;
            }
        }
        
        if (productionReloadNeeded && !info.ProductionInfoLoaded)
            await ProductionService.LoadVanBoardAsync(info);
        
        var json = JsonConvert.SerializeObject(
                            info, 
                            ZoneJsonSerializerSettings.GetOptionalSerializerSettings(includedFields)
                            );
        
        return Ok(json);
    }
}