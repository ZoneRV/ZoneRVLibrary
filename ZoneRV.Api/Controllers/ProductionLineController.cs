using Microsoft.AspNetCore.Mvc;
using ZoneRV.Models.Production;
using ZoneRV.Services.Production;

namespace ZoneRV.Api.Controllers;

[Route("api/production-line"), ApiController]
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

    [HttpGet("area-of-origins")]
    public ActionResult<IEnumerable<AreaOfOrigin>> AddAreaOfOrigin() //TODO Add better way of requesting fields
    {
        var areas = ProductionService.ProductionLines
                                     .SelectMany(x => x.AreaOfOrigins
                                                       .Select(x => new AreaOfOrigin()
                                                        {
                                                            Id = x.Id, 
                                                            Name = x.Name,
                                                            Line = new ProductionLine()
                                                            {
                                                                Name = x.Line.Name,
                                                                Id = x.Line.Id,
                                                                AreaOfOrigins = [],
                                                                Models = []
                                                            }
                                                        }));

        return Ok(areas);
    }

    [HttpGet("area-of-origin/{id}")]
    public ActionResult<AreaOfOrigin> AddAreaOfOrigin(int id)
    {
        var area = ProductionService.ProductionLines
                                     .SelectMany(x => x.AreaOfOrigins)
                                     .SingleOrDefault(x => x.Id == id);

        if (area is null)
            return NotFound();

        return Ok(area);
    }

    [HttpPost("area-of-origin/add/{lineId}/{name}")]
    public async Task<ActionResult<AreaOfOrigin>> AddAreaOfOrigin(int lineId, string name)
    {
        var line = ProductionService.ProductionLines
                                    .SingleOrDefault(x => x.Id == lineId);
        
        if (line is null)
            return NotFound();

        var area = await ProductionService.CreateAreaOfOrigin(line, name);

        return Ok(area);
    }

    [HttpPost("location/add-custom-name/{locationId}/{customName}")]
    public async Task<ActionResult<LocationCustomName>> AddCustomNameToLoation(int locationId, string customName)
    {
        var location = ProductionService.LocationFactory.Locations.SingleOrDefault(x => x.Id == locationId);

        if (location is null)
            return NotFound();

        var customNameO = await ProductionService.CreateCustomNameToLocation(location, customName);

        return Ok(customNameO);
    }
}