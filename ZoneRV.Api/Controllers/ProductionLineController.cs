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
    
    [HttpGet("{id}")]
    public ActionResult<ProductionLine?> GetLine(int id)
    {
        var line = ProductionService.ProductionLines.SingleOrDefault(x => x.Id == id);

        if (line is null)
            return NotFound();

        return Ok(line);
    }

    [HttpPost("add/{name}")]
    public async Task<ActionResult<ProductionLine>> AddLine(string name)
    {
        var newLine = await ProductionService.CreateProductionLine(name);

        return Ok(newLine);
    }

    [HttpGet("area-of-origin")]
    public ActionResult<IEnumerable<ProductionLine>> AddAreaOfOrigin()
    {
        var areas = ProductionService.ProductionLines.SelectMany(x => x.AreaOfOrigins);

        return Ok(areas);
    }

    [HttpPost("area-of-origin/{id}/{name}")]
    public async Task<ActionResult<ProductionLine>> AddAreaOfOrigin(int lineId, string areaName)
    {
        var line = ProductionService.ProductionLines.SingleOrDefault(x => x.Id == lineId);
        
        if (line is null)
            return NotFound();

        var newArea = await ProductionService.CreateAreaOfOrigin(line, areaName);

        return Ok(newArea);
    }
}