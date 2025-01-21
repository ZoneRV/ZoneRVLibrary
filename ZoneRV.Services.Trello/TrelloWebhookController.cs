using Microsoft.AspNetCore.Mvc;
using ZoneRV.Services.Production;

namespace ZoneRV.Services.Trello;

public class TrelloWebhookController : ControllerBase
{
    private IProductionService _productionService;
    
    public TrelloWebhookController(IProductionService productionService)
    {
        _productionService  = productionService;
    }

     //TODO Remove[IgnoreAntiforgeryToken]
    [HttpPost]
    public async Task<ActionResult> Post()
    {
        return Ok();
    }
}