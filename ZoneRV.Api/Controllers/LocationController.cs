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
}