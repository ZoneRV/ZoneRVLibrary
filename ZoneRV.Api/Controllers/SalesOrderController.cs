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
        IEnumerable<SalesOrder> infos = ProductionService.Where(options.FilterFunction());

        var infosList = infos.ToList();

        infosList = options.OrderFunction(infosList).ToList(); // TODO ideally vans should be loaded before sorting and after paging =(
        
        if (options.Pagination is not null)
        {
            if (options.Pagination.PageCount < 1)
                return BadRequest($"{nameof(options.Pagination.PageCount)} must be at least 1");
            
            infosList = infosList.Skip((int)options.Pagination.PageStartIndex).Take((int)options.Pagination.PageLimit).ToList();
        }

        if (options.OptionalFields is not null && BoardNeedsLoading(options.OptionalFields) && infosList.Count(x => !x.ProductionInfoLoaded) > 10)
        {
            return BadRequest("Too many unloaded vans requested, try loading less at once.");
        }
        
        var json = await SerializeSalesOrders(infosList, options.OptionalFields);

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
            await ProductionService.LoadSalesOrderBoardsAsync(salesOrders);

        List<SalesOrder> clones = [];

        if(includedFields is not null && includedFields.Any(x => x.ToLower() == "salesorder_stats"))
        {
            foreach (var salesOrder in salesOrders)
            {
                SalesOrder clone = (SalesOrder)salesOrder.Clone();
                clone.Stats = new SalesOrderStats(clone);

                clones.Add(clone);
            }
        }
        else
        {
            clones = salesOrders.ToList();
        }

        var json = JsonConvert.SerializeObject(
            clones, 
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
            await ProductionService.LoadSalesOrderBoardAsync(info);
        
        var json = JsonConvert.SerializeObject(
            info, 
            ZoneJsonSerializerSettings.GetOptionalSerializerSettings(includedFields)
        );
        
        return json;
    }
}