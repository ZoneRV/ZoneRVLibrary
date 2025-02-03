using Microsoft.AspNetCore.Mvc;
using ZoneRV.Models.Production;
using ZoneRV.Services.Production;

namespace ZoneRV.Api.Controllers;

[Route("api/workspace"), ApiController]
public class WorkspaceController : ControllerBase
{
    private IProductionService ProductionService { get; set; }
    
    public WorkspaceController(IProductionService productionService)
    {
        ProductionService = productionService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<ProductionWorkspace>> GetWorkspaces()
    {
        var workspaces = ProductionService.Workspaces;

        return Ok(workspaces);
    }

    [HttpGet("{id}")]
    public ActionResult<IEnumerable<ProductionWorkspace>> GetWorkspace(int id)
    {
        var workspace = ProductionService.Workspaces.SingleOrDefault(x => x.Id == id);

        if (workspace is null)
            return NotFound();

        return Ok(workspace);
    }

    [HttpPost("add/{name}")]
    public async Task<ActionResult<IEnumerable<ProductionWorkspace>>> GetWorkspace(string name, [FromQuery] string? description)
    {
        var workspace = await ProductionService.CreateProductionWorkspace(name, description);

        return Ok(workspace);
    }
    
    [HttpGet("production-line/")]
    public ActionResult<IEnumerable<ProductionLine>> GetLines()
    {
        var lines = ProductionService.ProductionLines;

        return Ok(lines);
    }
    
    [HttpGet("production-line/{id}")]
    public ActionResult<ProductionLine?> GetLine(int id)
    {
        var line = ProductionService.ProductionLines.SingleOrDefault(x => x.Id == id);

        if (line is null)
            return NotFound();

        return Ok(line);
    }

    [HttpPost("production-line/add/{workspaceId}")]
    public async Task<ActionResult<ProductionLine>> AddLine(int workspaceId, string name, string? description)
    {
        var workspace = ProductionService.Workspaces.SingleOrDefault(x => x.Id == workspaceId);

        if (workspace is null)
            return NotFound();
        
        var newLine = await ProductionService.CreateProductionLine(workspace, name, description);

        return Ok(newLine);
    }

    [HttpGet("production-line/area-of-origins")]
    public ActionResult<IEnumerable<AreaOfOrigin>> AddAreaOfOrigin()
    {
        var areas = ProductionService.ProductionLines
                                     .SelectMany(x => x.AreaOfOrigins);

        return Ok(areas);
    }

    [HttpGet("production-line/area-of-origin/{id}")]
    public ActionResult<AreaOfOrigin> AddAreaOfOrigin(int id)
    {
        var area = ProductionService.ProductionLines
                                     .SelectMany(x => x.AreaOfOrigins)
                                     .SingleOrDefault(x => x.Id == id);

        if (area is null)
            return NotFound();

        return Ok(area);
    }

    [HttpPost("production-line/area-of-origin/add/{lineId}")]
    public async Task<ActionResult<AreaOfOrigin>> AddAreaOfOrigin(int lineId, [FromQuery] string name)
    {
        var line = ProductionService.ProductionLines
                                    .SingleOrDefault(x => x.Id == lineId);
        
        if (line is null)
            return NotFound();

        var area = await ProductionService.CreateAreaOfOrigin(line, name);

        return Ok(area);
    }
}