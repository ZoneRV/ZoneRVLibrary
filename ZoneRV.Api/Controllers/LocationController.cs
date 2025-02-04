using FishbowlSQL.Models;
using Microsoft.AspNetCore.Mvc;
using ZoneRV.Services.Production;

namespace ZoneRV.Api.Controllers;

[Route("api/workspace/location"), ApiController]
public class LocationController : ControllerBase
{
    private IProductionService ProductionService { get; set; }
    
    public LocationController(IProductionService productionService)
    {
        ProductionService = productionService;
    }
    
    [HttpPost("add/{workspaceId}")]
    public async Task<ActionResult<WorkspaceLocation>> AddWorkSpaceLocation(
        int workspaceId, 
        [FromQuery] string locationName, 
        [FromQuery] ProductionLocationType type, 
        [FromQuery] string? description)
    {
        var workspace = ProductionService.Workspaces.SingleOrDefault(x => x.Id == workspaceId);

        if (workspace is null)
            return NotFound();

        var location = await ProductionService.CreateWorkspaceLocation(workspace, locationName, description, type);

        return Ok(location);
    }

    [HttpPost("line/add/{lineId}")]
    public async Task<ActionResult<OrderedLineLocation>> AddOrderedLineLocation(
        int lineId, 
        [FromQuery] int locationId, 
        [FromQuery] decimal order, 
        [FromQuery] string? customName, 
        [FromQuery] string? inventoryName)
    {
        var line     = ProductionService.ProductionLines.SingleOrDefault(x => x.Id == lineId);
        var location = ProductionService.LocationFactory.WorkspaceLocations.SingleOrDefault(x => x.Id == locationId);

        if (line is null)
            return NotFound();

        if (location is null)
            return NotFound();

        var newLocation = await ProductionService.CreateOrderedLocation(line, location, order, customName, inventoryName);

        return Ok(newLocation);
    }
}