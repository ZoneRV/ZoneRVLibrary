using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc;
using ZoneRV.Api.Models;
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

        var json = await SerializeSaleOrder(info, includedFields);
        return Ok(json);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalesOrder>>> Get(SalesOrderOptions options)
    {
        var infos =
            ProductionService.GetInfos(x =>
                (options.WorkspaceIds is null || options.WorkspaceIds.Contains(x.Model.Line.Workspace.Id)) &&
                (options.LineIds is null || options.LineIds.Contains(x.Model.Line.Id)) &&
                (options.ModelIds is null || options.ModelIds.Contains(x.Model.Id)) &&
                (options.Names is null || options.Names.Contains(x.Name)) &&
                (options.Ids is null || x.Id is null || options.Ids.Contains(x.Id)) &&
                (options.OrderedLocationId is null || x.LocationInfo.CurrentLocation is not null && options.OrderedLocationId.Contains(x.LocationInfo.CurrentLocation.Id)) &&
                (options.WorkspaceLocationId is null || x.LocationInfo.CurrentLocation is not null && options.WorkspaceLocationId.Contains(x.LocationInfo.CurrentLocation.Location.Id))
            ).ToList();

        if (options.OptionalFields is not null && BoardNeedsLoading(options.OptionalFields) && infos.Count(x => !x.ProductionInfoLoaded) > 10)
        {
            return BadRequest("Too many unloaded vans requested, try loading less at once.");
        }

        var json = await SerializeSalesOrders(infos, options.OptionalFields);

        if (json.Length > 10000000)
            return BadRequest("Request too large");
        
        return Ok(json);
    }

    private async Task<string> SerializeSalesOrders(IEnumerable<SalesOrder> salesOrders, List<string>? includedFields = null)
    {
        bool productionReloadNeeded = false;
        
        foreach (var field in includedFields ?? [])
        {
            SerializationUtils.CheckForRequired(field, out var prod, out _);

            if (prod)
            {
                productionReloadNeeded = true;
                break;
            }
        }
        
        if(productionReloadNeeded)
            await ProductionService.LoadVanBoardsAsync(salesOrders);

        var json = JsonConvert.SerializeObject(
            salesOrders, 
            ZoneJsonSerializerSettings.GetOptionalSerializerSettings(includedFields)
        );
        
        return json;
    }

    private bool BoardNeedsLoading(IEnumerable<string> includedFields)
    {
        foreach (var field in includedFields)
        {
            SerializationUtils.CheckForRequired(field, out var prod, out _);

            if (prod)
            {
                return true;
            }
        }

        return false;
    }

    private async Task<string> SerializeSaleOrder(SalesOrder info, List<string>? includedFields = null)
    {
        includedFields = includedFields ?? [];

        if (BoardNeedsLoading(includedFields) && !info.ProductionInfoLoaded)
            await ProductionService.LoadVanBoardAsync(info);
        
        var json = JsonConvert.SerializeObject(
            info, 
            ZoneJsonSerializerSettings.GetOptionalSerializerSettings(includedFields)
        );
        
        return json;
    }
}