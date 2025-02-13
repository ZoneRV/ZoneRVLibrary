using System.Threading.Channels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using TrelloDotNet;
using ZoneRV.Models.Enums;
using ZoneRV.Services.Production;

namespace ZoneRV.Services.Trello;

[Route("api/trello-webhook"), ApiController]
public class TrelloWebhookController : ControllerBase
{
    private          TrelloService       TrelloService { get; set; }
    private readonly WebhookDataReceiver _receiver;
    public           Channel<BaseUpdate> WebhookChanel { get; set; }
    
    public TrelloWebhookController(IProductionService productionService, Channel<BaseUpdate> webhookChannel)
    {
        WebhookChanel = webhookChannel;
        
        if (productionService is not TrelloService trelloService)
            throw new Exception("Must use trello service with trello webhook controller");

        TrelloService = trelloService;

        _receiver = new WebhookDataReceiver(TrelloService.TrelloClient);
    }

    [HttpHead]
    public ActionResult Head()
    {
        return Ok();
    }
    
    [IgnoreAntiforgeryToken] // TODO: re add anti forgery token
    [HttpPost]
    public async Task<ActionResult> Post()
    {
        if (!TrelloService.WebhooksEnabled)
        {
            return Ok();
        }
        
        // Check Events
        _receiver.BasicEvents.OnCreateCheckItem            += async args => await WebhookChanel.Writer.WriteAsync(args.ToCheckUpdatedData(EntityUpdateType.Add));    // TODO: TEST
        _receiver.BasicEvents.OnDeleteCheckItem            += async args => await WebhookChanel.Writer.WriteAsync(args.ToCheckUpdatedData(EntityUpdateType.Remove)); // TODO: TEST
        _receiver.BasicEvents.OnUpdateCheckItemStateOnCard += async args => await WebhookChanel.Writer.WriteAsync(args.ToCheckUpdatedData(EntityUpdateType.Update)); // TODO: TEST
        _receiver.BasicEvents.OnUpdateCheckItem            += async args => await WebhookChanel.Writer.WriteAsync(args.ToCheckUpdatedData(EntityUpdateType.Update)); // TODO: TEST

        // Checklist Events
        _receiver.BasicEvents.OnAddChecklistToCard      += async args => await WebhookChanel.Writer.WriteAsync(args.ToChecklistUpdatedData(EntityUpdateType.Add));    // TODO: TEST
        _receiver.BasicEvents.OnRemoveChecklistFromCard += async args => await WebhookChanel.Writer.WriteAsync(args.ToChecklistUpdatedData(EntityUpdateType.Remove)); // TODO: TEST
        _receiver.BasicEvents.OnCopyChecklist           += async args => await WebhookChanel.Writer.WriteAsync(args.ToChecklistUpdatedData(EntityUpdateType.Copy));   // TODO: TEST
        _receiver.BasicEvents.OnUpdateChecklist         += async args => await WebhookChanel.Writer.WriteAsync(args.ToChecklistUpdatedData(EntityUpdateType.Update)); // TODO: TEST

        // Card Events
        _receiver.BasicEvents.OnCreateCard               += (args =>  throw new NotImplementedException()); // TODO: TEST
        _receiver.BasicEvents.OnDeleteCard               += (args =>  throw new NotImplementedException()); // TODO: TEST
        _receiver.BasicEvents.OnCopyCard                 += (args =>  throw new NotImplementedException()); // TODO: TEST
        _receiver.BasicEvents.OnUpdateCard               += (args =>  throw new NotImplementedException()); // TODO: TEST
        _receiver.BasicEvents.OnUpdateCustomFieldItem    += (args =>  throw new NotImplementedException()); // TODO: TEST
        _receiver.BasicEvents.OnAddAttachmentToCard      += (args =>  throw new NotImplementedException()); // TODO: TEST
        _receiver.BasicEvents.OnDeleteAttachmentFromCard += (args =>  throw new NotImplementedException()); // TODO: TEST
        _receiver.BasicEvents.OnAddMemberToCard          += (args =>  throw new NotImplementedException()); // TODO: TEST
        _receiver.BasicEvents.OnRemoveMemberFromCard     += (args =>  throw new NotImplementedException()); // TODO: TEST

        // Comment Events
        _receiver.BasicEvents.OnCommentCard   += (args =>  throw new NotImplementedException()); // TODO: TEST
        _receiver.BasicEvents.OnDeleteComment += (args =>  throw new NotImplementedException()); // TODO: TEST
        _receiver.BasicEvents.OnUpdateComment += (args =>  throw new NotImplementedException()); // TODO: TEST
        
        //User events
        _receiver.BasicEvents.OnUpdateMember                 += (args =>  throw new NotImplementedException()); // TODO: TEST
        _receiver.BasicEvents.OnRemoveMemberFromOrganization += (args =>  throw new NotImplementedException()); // TODO: TEST
        _receiver.BasicEvents.OnAddMemberToOrganization      += (args =>  throw new NotImplementedException()); // TODO: TEST
        
        // List Events
        _receiver.BasicEvents.OnUpdateList        += (args =>  throw new NotImplementedException()); // TODO: TEST
        _receiver.BasicEvents.OnMoveListToBoard   += (args =>  throw new NotImplementedException()); // TODO: TEST
        _receiver.BasicEvents.OnMoveListFromBoard += (args =>  throw new NotImplementedException()); // TODO: TEST
            
        //TODO figure out handle board closing

        try
        {
            using var streamReader = new StreamReader(Request.Body);
            string json = await streamReader.ReadToEndAsync();

            if (json != string.Empty)
            {
                var notification = _receiver.ConvertJsonToWebhookNotification(json);

                Log.Logger.Debug("Webhook received [{type}]", notification.Action.Type);

                _receiver.ProcessJsonIntoEvents(json);
            }

            return Ok();
        }
        catch (Exception ex)
        {
            Log.Logger.Error(ex, "Exception occured during Trello webhook.");
            return StatusCode(500);
        }
    }
}