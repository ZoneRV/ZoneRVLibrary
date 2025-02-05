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
    public async Task<ActionResult<IEnumerable<SalesOrder>>> Get(GroupRequestById requests)
    {
        var infos = ProductionService.GetInfos(x => requests.Names.Contains(x.Name));

        var json = await SerializeSalesOrders(infos, requests.OptionalFields);

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

    private async Task<string> SerializeSaleOrder(SalesOrder info, List<string>? includedFields = null)
    {
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
        
        return json;
    }
}