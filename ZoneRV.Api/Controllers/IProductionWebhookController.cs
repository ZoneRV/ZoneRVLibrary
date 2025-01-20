using Microsoft.AspNetCore.Mvc;
using ZoneRV.Models.UpdateModes;
using ZoneRV.Services.Production;

namespace ZoneRV.Api.Controllers;

public abstract class IProductionWebhookController
{
    public IProductionService ProductionService { get; set; }
    
    public IProductionWebhookController(IProductionService productionService)
    {
        ProductionService = productionService;
    }

    [HttpHead]
    public async Task<ActionResult> Head()
    {
        return new OkResult();
    }

    private void UpdateCheck(CheckUpdated check)
    {
        
    }
}