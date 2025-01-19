using Microsoft.Extensions.Configuration;
using Serilog;
using TrelloDotNet;
using TrelloDotNet.Model;
using TrelloDotNet.Model.Actions;
using TrelloDotNet.Model.Options;
using TrelloDotNet.Model.Options.GetActionsOptions;
using TrelloDotNet.Model.Options.GetCardOptions;
using TrelloDotNet.Model.Options.GetMemberOptions;
using TrelloDotNet.Model.Search;
using ZoneRV.Models;
using ZoneRV.Models.DB;
using ZoneRV.Models.Enums;
using ZoneRV.Models.Location;
using ZoneRV.Services.Production;
using ZoneRV.Services.Trello.DB;
using ZoneRV.Services.Trello.Models;
using TrelloActionData = ZoneRV.Services.Trello.DB.TrelloActionData;

namespace ZoneRV.Services.Trello;

public class TrelloService : IProductionService
{
    public VanIdData VanIdData { get; } 
    public TrelloActionData TrelloActionData { get; } 
    
    public sealed override LocationFactory LocationFactory { get; init; }
    
    public static List<string> TrelloActionFilters => ["commentCard", "updateCustomFieldItem", "createCard", "updateCheckItemStateOnCard"];
    
    private TrelloClient TrelloClient     { get; }
    private string       TrelloApiKey     { get; }
    private string       TrelloUserToken  { get; }
    private string       LineMoveBoardId  { get; }
    private string       ProHoDashboardId { get; }

    public override int MaxDegreeOfParallelism { get; protected set; } = 3;
    
    public TrelloService(IConfiguration configuration, ProductionDataService productionDataService, LocationData locationData, VanIdData vanIdData, TrelloActionData trelloActionData) : base(configuration, productionDataService)
    {
        VanIdData = vanIdData;
        TrelloActionData = trelloActionData;
        
        TrelloApiKey = configuration["TrelloApiKey"] ??
                       throw new ArgumentNullException(nameof(TrelloApiKey), "Trello api key required");
        
        TrelloUserToken = configuration["TrelloUserToken"] ??
                       throw new ArgumentNullException(nameof(TrelloUserToken), "Trello user token required");
        
        LineMoveBoardId = configuration["LineMoveBoardId"] ??
                          throw new ArgumentNullException(nameof(LineMoveBoardId), "Line move board id required");
        
        ProHoDashboardId = configuration["ProHoDashboardId"] ??
                        throw new ArgumentNullException(nameof(ProHoDashboardId), "ProHo Dashboard board id required");

        var locations = locationData.GetLocations(CustomNameSource.Trello).Result; // TODO: Fix this madness

        LocationFactory = new LocationFactory
        {
            Locations = new LocationCollection(locations),
            IgnoredListNames = [] // TODO: fill out
        };
        
        var clientOptions = new TrelloClientOptions
        {
            AllowDeleteOfBoards = false,
            AllowDeleteOfOrganizations = false,
            MaxRetryCountForTokenLimitExceeded = 3
        };

        TrelloClient = new TrelloClient(TrelloApiKey, TrelloUserToken, clientOptions);
    }

    public override async Task InitialiseService()
    {
        Member member;

        try
        {
            member = await TrelloClient.GetTokenMemberAsync();
            Log.Logger.Information("Trello Successfully connected as {member}", member.Username);
        }
        catch (Exception ex)
        {
            Log.Logger.Fatal(ex, "Trello client could not connect");

            throw;
        }

        var organisations = await TrelloClient.GetOrganizationsForMemberAsync(member.Id);
        
        await LoadUsers(organisations);

        await LoadBasicProductionInfo();
    }

    private async Task LoadUsers(IEnumerable<Organization> organisations)
    {
        foreach (Organization organization in organisations)
        {
            GetMemberOptions getMemberOptions = new GetMemberOptions
            {
                MemberFields = new MemberFields(
                    MemberFieldsType.FullName,
                    MemberFieldsType.Username,
                    MemberFieldsType.AvatarUrl
                )
            };

            List<Member> members = await TrelloClient.GetMembersOfOrganizationAsync(organization.Id, getMemberOptions);

            foreach (Member orgMember in members)
            {
                if (!Users.ContainsKey(orgMember.Id))
                {
                    User newMember = new User()
                    {
                        Id = orgMember.Id,
                        Username = orgMember.Username,
                        AvatarUrl = orgMember.AvatarUrl,
                        FullName = orgMember.FullName
                    };
                        
                    Users.TryAdd(orgMember.Id, newMember);

                    Log.Logger.Debug("New member [{name}] added to production service.", newMember.FullName);
                }
            }
        }
    }

    private async Task LoadBasicProductionInfo()
    {
        List<Card>         lineMoveBoardCards = await TrelloClient.GetCardsOnBoardAsync(LineMoveBoardId, new GetCardOptions() {IncludeList = true});
        List<TrelloAction> lineMoveActions    = await GetTrelloActions(LineMoveBoardId, ["updateCard:idList"]);
        
        List<Card>         ccDashboardCards   = await TrelloClient.GetCardsOnBoardAsync(ProHoDashboardId, new GetCardOptions());

        var cachedActions = await GetTrelloActionsWithCache(ProHoDashboardId);

        List<List>? lists = await TrelloClient.GetListsOnBoardAsync(LineMoveBoardId);
        List<string> modelPrefixes = ProductionLines.SelectMany(x => x.Models.Select(y => y.Prefix.ToLower())).ToList();

        lineMoveBoardCards = lineMoveBoardCards
                             .Where(c 
                                 => modelPrefixes
                                     .Any(vm 
                                        => c.Name.ToLower().Contains(vm.ToLower()) 
                                    )).ToList();
        
        ccDashboardCards = ccDashboardCards
                            .Where(c 
                                => modelPrefixes
                                    .Any(vm 
                                        => c.Name.ToLower().Contains(vm.ToLower()) 
                                    )).ToList();

        List<string> tempBlockedNames = [];

        IEnumerable<VanId> storedVanIds = await VanIdData.GetIds();
        List<VanId> storedVanIdList = storedVanIds.ToList();

        foreach (Card card in lineMoveBoardCards)
        {
            
            if (ModelNameMatcher.TryGetVanName(card.Name, out var model, out string? formattedName))
            {
                if (tempBlockedNames.Contains(formattedName))
                    continue;

                if (Vans.ContainsKey(formattedName))
                {
                    Vans.TryRemove(formattedName, out _);
                    tempBlockedNames.Add(formattedName);

                    Log.Logger.Error("{name} found at least twice in line move board. Ignoring both until issue is resolved.", formattedName);
                    
                    continue;
                }

                VanId? vanId = storedVanIdList.SingleOrDefault(x => x.VanName == formattedName);
                string? idString;
                string? urlString;
                
                if(vanId is not null && vanId.Blocked)
                    continue;

                if (vanId is null || string.IsNullOrEmpty(vanId.Id) || string.IsNullOrEmpty(vanId.Url))
                {
                    TimeSpan lastUpdated = DateTimeOffset.UtcNow - card.LastActivity.UtcDateTime;
                    (bool boardfound, VanId? vanId) search = await TrySearchForVanId(formattedName, lastUpdated);

                    if (!search.boardfound || search.vanId is null)
                        continue;

                    else
                    {
                        idString = search.vanId.Id;
                        urlString = search.vanId.Url;
                    }
                }
                else
                {
                    idString = vanId.Id;
                    urlString = vanId.Url;
                }

                List<(DateTimeOffset moveDate, ProductionLocation location)> locationHistory = [];
                
                foreach (var moveAction in lineMoveActions)
                {
                    List list;

                    if (lists.Count(x => x.Id == moveAction.Data.ListAfter.Id) == 1)
                        list = lists.Single(x => x.Id == moveAction.Data.ListAfter.Id);

                    else
                        continue;

                    string listName = list.Name;

                    if (listName == "SCHEDULED VANS (50x)" || listName == "SCHEDULED EXPO VANS")
                    {
                        locationHistory.Add((moveAction.Date, LocationFactory.PreProduction));
                        continue;
                    }

                    var location = LocationFactory.GetLocationFromCustomName(model.ProductionLine, listName);
                    
                    if(location is null)
                        continue;
                    
                    locationHistory.Add((moveAction.Date, location));
                }

                Vans.TryAdd(formattedName, new VanProductionInfo()
                {
                    Name = formattedName, 
                    VanModel = model, 
                    Url = urlString, 
                    Id = idString,
                    LocationInfo = new VanLocationInfo()
                    {
                        LocationHistory = locationHistory
                    }
                });
                
                Log.Logger.Debug("New van information added");
            }
            else
            {
                Log.Logger.Debug("Does not represent a van, ignoring.");
            }
        }

        Log.Logger.Information("{vanCount} Vans added to the production service. Adding handover dates...", Vans.Count);

        foreach (Card card in ccDashboardCards)
        {
            
            if (ModelNameMatcher.TryGetVanName(card.Name, out _, out string? formattedName))
            {
                if (!card.Due.HasValue)
                    continue;

                if (Vans.TryGetValue(formattedName, out VanProductionInfo? value))
                {
                    List<CachedTrelloAction> actions = cachedActions.Where(x => x.CardId == card.Id && x.DueDate.HasValue).ToList();

                    foreach (CachedTrelloAction action in actions.OrderBy(x => x.DateOffset))
                    {
                        value.AddHandoverHistory(action.DateOffset, action.DueDate!.Value);
                    }

                    if (card.Due.HasValue)
                    {
                        value.HandoverState =
                            card.DueComplete ? HandoverState.HandedOver : HandoverState.UnhandedOver;
                        
                        var dueUpdatedAction = actions.LastOrDefault(x => x.DueDate.HasValue);
                        
                        if(dueUpdatedAction is not null)
                            value.HandoverStateLastUpdated = dueUpdatedAction.DateOffset;
                    }

                    Log.Logger.Debug("Added {handover} to {vanName} ({handoverStat})", card.Due.Value.LocalDateTime.Date.ToString("dd/MM/yy"), value.Name, value.HandoverState);
                }
            }
            else
            {
                Log.Logger.Debug("Does not represent a van, ignoring.");
            }
        }

        Log.Logger.Information("{vanCount} handover dates added", Vans.Count(x => x.Value.HandoverDate.HasValue));

        foreach (var productionLine in ProductionLines)
        {
            var vansInLine = Vans.Where(x => x.Value.VanModel.ProductionLine == productionLine).Select(x => x.Value).ToList();
                
            int prepCount = vansInLine.Count(x =>
                x.LocationInfo.CurrentLocation.Type is ProductionLocationType.Prep);
                
            int prodCount = vansInLine.Count(x =>
                x.LocationInfo.CurrentLocation.Type is ProductionLocationType.Bay or ProductionLocationType.Module or ProductionLocationType.Subassembly);
                
            int finishingCount = vansInLine.Count(x =>
                x.LocationInfo.CurrentLocation.Type is ProductionLocationType.Finishing && x.HandoverState is not HandoverState.HandedOver);
            
            Log.Logger.Information("{line}: Prep: {prepCount} - In Production: {prodCount} - In Finishing: {finishingCount}", productionLine.Name, prepCount, prodCount, finishingCount);
        }
    }

    protected override async Task<VanProductionInfo> _loadVanFromSourceAsync(VanProductionInfo van)
    {
        GetCardOptions getCardOptions = new GetCardOptions
        {
            IncludeChecklists       = true,
            ChecklistFields         = ChecklistFields.All,
            IncludeList             = true,
            IncludeBoard            = true,
            BoardFields             = new BoardFields(BoardFieldsType.Name),
            IncludeCustomFieldItems = true,
            IncludeAttachments      = GetCardOptionsIncludeAttachments.True,
            CardFields = new CardFields(
                                        CardFieldsType.Name,
                                        CardFieldsType.Position,
                                        CardFieldsType.ChecklistIds,
                                        CardFieldsType.Closed,
                                        CardFieldsType.ListId,
                                        CardFieldsType.MemberIds)
        };
        
        List<Card> cards;
        try
        { 
            cards = await TrelloClient.GetCardsOnBoardAsync(van.Id, getCardOptions);
        }
        catch (Exception ex) //TODO: Handle "TrelloDotNet.Model.TrelloApiException: The requested resource was not found" exception
        {
            if (ex is TrelloApiException && ex.Message == "invalid id")
            {
                Log.Logger.Warning("{vanName} board id no longer exists. Refreshing Cached id.", van.Name);

                await VanIdData.DeleteVanId(van.Name);

                (bool boardfound, VanId? vanId) newId = await TrySearchForVanId(van.Name);

                if (!newId.boardfound || newId.vanId is null || string.IsNullOrEmpty(newId.vanId.Id) || string.IsNullOrEmpty(newId.vanId.Url))
                    throw new Exception("Board could not be found");
                
                van.Id = newId.vanId.Id;
                van.Url = newId.vanId.Url;
                cards = await TrelloClient.GetCardsOnBoardAsync(newId.vanId.Id, getCardOptions);
            }
            else
            {
                throw;
            }
        }
        
        List<CustomField> customFields = await TrelloClient.GetCustomFieldsOnBoardAsync(van.Id);

        var cachedActions = await GetTrelloActionsWithCache(van.Id!);

        foreach (var card in cards)
        {
            var cardActions = cachedActions.Where(x => x.BoardId == van.Id).ToList();
            var cardFields = customFields.Where(x => x.ModelId == van.Id).ToList();

            CardType cardType = TrelloUtils.GetCardType(card.Name, card.List.Name);

            switch (cardType)
            {
                case CardType.JobCard:
                    var position = LocationFactory.GetLocationFromCustomName(van.VanModel.ProductionLine, card.List.Name);
                    if(position is not null)
                        CreateJobCard(van, card.ToJobCardInfo(cardActions, cardFields), TrelloUtils.ToAreaOfOrigin(card, cardFields), position);
                    break;
            
                case CardType.RedCard:
                    CreateRedCard(van, card.ToRedCardInfo(cardActions, cardFields), TrelloUtils.ToAreaOfOrigin(card, cardFields));
                    break;
            
                case CardType.YellowCard:
                    CreateYellowCard(van, card.ToYellowCardInfo(cardActions, cardFields), TrelloUtils.ToAreaOfOrigin(card, cardFields));
                    break;
            }
        }

        return van;
    }


    private async Task<List<CachedTrelloAction>> GetTrelloActionsWithCache(string id, List<string>? actionFilters = null)
    {
        GetActionsOptions getActionsOptions = new GetActionsOptions()
        {
            Limit = 1000,
            Filter = actionFilters ?? TrelloActionFilters
        };
        
        IEnumerable<CachedTrelloAction> cachedActions = await TrelloActionData.GetActions(id);
        cachedActions = cachedActions.ToList();
        
        List<TrelloAction> actionsToCache = [];

        if (!cachedActions.Any())
            getActionsOptions.Since = cachedActions.Last().ActionId;
       
        List<TrelloAction> newActions = await TrelloClient.GetActionsOfBoardAsync(id, getActionsOptions);

        actionsToCache.AddRange(newActions);
        
        while (newActions.Count == 1000)
        {
            string lastId = newActions.Last().Id;
            
            if(!cachedActions.Any())
            {
                getActionsOptions.Before = lastId;
            }
            
            else
            {
                getActionsOptions.Before = lastId;
                getActionsOptions.Since = cachedActions.Last().ActionId;
            }
            
            newActions = await TrelloClient.GetActionsOfBoardAsync(id, getActionsOptions);
            
            actionsToCache.AddRange(newActions);
        }
        
        if (actionsToCache.Count > 0)
        {
            var returnedActions = await TrelloActionData.InsertTrelloActions(actionsToCache.ToCachedTrelloActions());

            cachedActions = returnedActions.Concat(cachedActions);
        }

        return cachedActions.ToList();
    }

    private async Task<List<TrelloAction>> GetTrelloActions(string id, List<string>? filteredActions = null)
    {
        GetActionsOptions getActionsOptions = new GetActionsOptions()
        {
            Limit = 1000,
            Filter = filteredActions ?? TrelloActionFilters
        };
        
        List<TrelloAction> actions = [];
       
        List<TrelloAction> newActions = await TrelloClient.GetActionsOfBoardAsync(id, getActionsOptions);

        actions.AddRange(newActions);
        
        while (newActions.Count == 1000)
        {
            string lastId = newActions.Last().Id;
            
            getActionsOptions.Before = lastId;
            
            newActions = await TrelloClient.GetActionsOfBoardAsync(id, getActionsOptions);
            
            actions.AddRange(newActions);
        }

        return actions;
    }
    
    private async Task<(bool boardfound, VanId? vanId)> TrySearchForVanId(string name, TimeSpan? age = null)
    {
        if (TrelloClient is null)
            throw new Exception("Trello Client has not been initialized.");

        VanId? vanId = await VanIdData.GetId(name);

        if (vanId is null)
        {
            vanId = new VanId(name);
            await VanIdData.InsertVanId(vanId);
        }
        else
        {
            if (vanId.Blocked)
                return (false, null);

            if (!string.IsNullOrEmpty(vanId.Id) && !string.IsNullOrEmpty(vanId.Url))
                return (true, null);
        }

        SearchRequest searchRequest = new SearchRequest(name)
                                      {
                                          SearchCards = false,
                                          BoardFields = new SearchRequestBoardFields("name", "closed", "url", "shortUrl")
                                      };

        SearchResult searchResults = await TrelloClient.SearchAsync(searchRequest);

        List<Board> results = searchResults.Boards.Where(x => !x.Closed).ToList();

        if (results.Count > 1)
        {
            Log.Logger.Error("Multiple Boards found for van {name}, not adding to cache - {urlList}", name, string.Join(", ", results.Select(x => $"https://trello.com/b/{x.Id}/")));

            return (false, null);
        }

        if (!results.Any())
        {
            if (age.HasValue && age > TimeSpan.FromDays(90))
            {
                vanId.Blocked = true;
                await VanIdData.UpdateVanId(vanId);
                
                Log.Logger.Warning("No trello search result for {name}, blocking van from future searches", name);
            }
            else
                Log.Logger.Warning("No trello search result for {name}", name);

            
            return (false, null);
        }

        vanId.Id = results.First().Id;
        vanId.Url = results.First().Url;
        
        await VanIdData.UpdateVanId(vanId);

        var id = await VanIdData.GetId(name);
        
        return (true, id);
    }
}