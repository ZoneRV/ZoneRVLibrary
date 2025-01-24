using Microsoft.AspNetCore.Mvc;
using ZoneRV.Models.Production;
using ZoneRV.Services.Production;

namespace ZoneRV.Api.Controllers;

[Route("api/[controller]"), ApiController]
public class ProductionLineController : ControllerBase
{
    private IProductionService ProductionService { get; set; }
    
    public ProductionLineController(IProductionService productionService)
    {
        ProductionService = productionService;
    }
    
    [HttpGet]
    public ActionResult<IEnumerable<ProductionLine>> GetLines()
    {
        var lines = ProductionService.ProductionLines;

        return Ok(lines);
    }

    [HttpPost("name")]
    public async Task<ActionResult<ProductionLine>> AddLine(string name)
    {
        var newLine = await ProductionService.CreateProductionLine(name);

        return Ok(newLine);
    }
}