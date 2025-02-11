using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ZoneRV.Api.Models;
using ZoneRV.Services.Production;

namespace ZoneRV.Api.Controllers;

[Route("api/misc"), ApiController]
public class MiscController : ControllerBase
{
    private IProductionService ProductionService { get; set; }
    
    public MiscController(IProductionService productionService)
    {
        ProductionService = productionService;
    }

    [HttpGet("sales-order/tare/{name}")]
    public async Task<ActionResult<SalesOrderTare>> GetTare(string name)
    {
        var salesOrder = ProductionService.FirstOrDefault(x => x.Name == name.ToLower());

        if (salesOrder is null)
            return NotFound();

        if (!salesOrder.ProductionInfoLoaded)
            await ProductionService.LoadSalesOrderBoardAsync(salesOrder);

        if (string.IsNullOrEmpty(salesOrder.Vin))
            return NotFound("Sales order does not have a VIN");

        using (var http = new HttpClient())
        {
            var response = await http.GetAsync($"https://www.rover.infrastructure.gov.au/RAVPublicSearch/?vinno={salesOrder.Vin}");

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();

            var tare   = Regex.Match(content, @"<div id=""tare"" class=""question-label"">(\d+)</div>");
            var gtm    = Regex.Match(content, @"<div id=""gtm"" class=""question-label"">(\d+)</div>");
            var gvmatm = Regex.Match(content, @"<div id=""gvmatm"" class=""question-label"">(\d+)</div>");

            if (tare.Success && gtm.Success && gvmatm.Success)
            {
                return Ok(new SalesOrderTare()
                {
                    Name    = salesOrder.Name,
                    Vin     = salesOrder.Vin,
                    Tare    = int.Parse(tare.Groups[1].Value),
                    GTM     = int.Parse(gtm.Groups[1].Value),
                    GVM_ATM = int.Parse(gvmatm.Groups[1].Value),
                });
            }
            else
            {
                return NotFound();
            }
        }
    }

    [HttpGet("sales-order/tares/")]
    public async Task<ActionResult<IEnumerable<SalesOrderTare>>> GetTares([FromBody] SalesOrderOptions options)
    {
        List<SalesOrder> salesOrders = ProductionService.Where(options.FilterFunction()).ToList();

        salesOrders = options.OrderFunction(salesOrders).ToList(); // TODO ideally vans should be loaded before sorting and after paging =(
        
        if (options.Pagination is not null)
        {
            if (options.Pagination.PageCount < 1)
                return BadRequest($"{nameof(options.Pagination.PageCount)} must be at least 1");
            
            salesOrders = salesOrders.Skip((int)options.Pagination.PageStartIndex).Take((int)options.Pagination.PageLimit).ToList();
        }

        if (salesOrders.Count(x => !x.ProductionInfoLoaded) > 10)
        {
            return BadRequest("Too many unloaded vans requested, try loading less at once.");
        }

        await ProductionService.LoadSalesOrderBoardsAsync(salesOrders);

        List<SalesOrderTare> results = [];

        foreach (var salesOrder in salesOrders)
        {
            if (string.IsNullOrEmpty(salesOrder.Vin))
            {
                results.Add(new SalesOrderTare(){Name = salesOrder.Name});
                continue;
            }

            using (var http = new HttpClient())
            {
                var response = await http.GetAsync($"https://www.rover.infrastructure.gov.au/RAVPublicSearch/?vinno={salesOrder.Vin}");

                var soTare =
                    new SalesOrderTare()
                    {
                        Name = salesOrder.Name, Vin = salesOrder.Vin
                    };
                
                if (!response.IsSuccessStatusCode)
                {
                    results.Add(soTare);
                    
                    continue;
                }

                var content = await response.Content.ReadAsStringAsync();

                var tare   = Regex.Match(content, @"<div id=""tare"" class=""question-label"">(\d+)</div>");
                var gtm    = Regex.Match(content, @"<div id=""gtm"" class=""question-label"">(\d+)</div>");
                var gvmatm = Regex.Match(content, @"<div id=""gvmatm"" class=""question-label"">(\d+)</div>");

                if (tare.Success)
                    soTare.Tare = int.Parse(tare.Groups[1].Value);
                
                if (gtm.Success)
                    soTare.GTM = int.Parse(gtm.Groups[1].Value);
                
                if (gvmatm.Success)
                    soTare.GVM_ATM = int.Parse(gvmatm.Groups[1].Value);
                
                results.Add(soTare);
            }
        }

        return Ok(results);
    }
}

public class SalesOrderTare
{
    public required string Name { get; set; }
    public string? Vin  { get; set; }
    public int?    Tare { get; set; }
    public int?    GTM  { get; set; }
    public int? GVM_ATM { get; set; }
}