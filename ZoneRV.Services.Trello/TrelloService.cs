using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TrelloDotNet;
using TrelloDotNet.Model;
using TrelloDotNet.Model.Actions;
using TrelloDotNet.Model.Options;
using TrelloDotNet.Model.Options.GetActionsOptions;
using TrelloDotNet.Model.Options.GetCardOptions;
using TrelloDotNet.Model.Options.GetMemberOptions;
using TrelloDotNet.Model.Search;
using ZoneRV.DBContexts;
using ZoneRV.Models;
using ZoneRV.Models.Enums;
using ZoneRV.Models.Location;
using ZoneRV.Models.Production;
using ZoneRV.Services.Production;
using ZoneRV.Services.Trello.Models;
using Card = TrelloDotNet.Model.Card;

namespace ZoneRV.Services.Trello;

public class TrelloService : IProductionService
{
    private readonly       IServiceScopeFactory _scopeFactory;
    public sealed override LocationFactory      LocationFactory { get; init; }
    
    public static List<string> TrelloActionFilters => ["commentCard", "updateCustomFieldItem", "createCard", "updateCheckItemStateOnCard"];
    
    protected override string       LocationTypeName { get => "trello"; }
    
    private            TrelloClient TrelloClient     { get; }
    private            string       TrelloApiKey     { get; }
    private            string       TrelloUserToken  { get; }
    private            string       LineMoveBoardId  { get; }
    private            string       ProHoDashboardId { get; }

    public override int MaxDegreeOfParallelism { get; protected set; } = 3;
    
    public TrelloService(IServiceScopeFactory scopeFactory, IConfiguration configuration) : base(scopeFactory, configuration)
    {
        _scopeFactory = scopeFactory;
        
        TrelloApiKey = configuration["TrelloApiKey"] ??
                       throw new ArgumentNullException(nameof(TrelloApiKey), "Trello api key required");
        
        TrelloUserToken = configuration["TrelloUserToken"] ??
                       throw new ArgumentNullException(nameof(TrelloUserToken), "Trello user token required");
        
        LineMoveBoardId = configuration["LineMoveBoardId"] ??
                          throw new ArgumentNullException(nameof(LineMoveBoardId), "Line move board id required");
        
        ProHoDashboardId = configuration["ProHoDashboardId"] ??
                        throw new ArgumentNullException(nameof(ProHoDashboardId), "ProHo Dashboard board id required");


        List<Location> locations;
        
        using (var scope = scopeFactory.CreateScope())
        {
            var productionContext = scope.ServiceProvider.GetRequiredService<ProductionContext>();
            
            locations= productionContext.Locations
                             .Include(l => l.Line)
                             .Include(l => l.CustomLocationNames.Where(cn => cn.ServiceType == LocationTypeName))
                             .Include(l => l.InventoryLocations).ToList();
        }
            

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
        List<TrelloAction> lineMoveActions    = await TrelloClient.GetActionsOfBoardAsync(LineMoveBoardId, new GetActionsOptions() { Limit = 1000, Filter = ["updateCard:idList"]});
        
        List<Card>         prohoCards   = await TrelloClient.GetCardsOnBoardAsync(ProHoDashboardId, new GetCardOptions());

        var prohoCachedActions = await GetTrelloActionsWithCache(ProHoDashboardId, ["updateCard"]);

        List<List>? lists = await TrelloClient.GetListsOnBoardAsync(LineMoveBoardId);
        List<string> modelPrefixes = ProductionLines.SelectMany(x => x.Models.Select(y => y.Prefix.ToLower())).ToList();

        lineMoveBoardCards = lineMoveBoardCards
                             .Where(c 
                                 => modelPrefixes
                                     .Any(vm 
                                        => c.Name.ToLower().Contains(vm.ToLower()) 
                                    )).ToList();
        
        prohoCards = prohoCards
                            .Where(c 
                                => modelPrefixes
                                    .Any(vm 
                                        => c.Name.ToLower().Contains(vm.ToLower()) 
                                    )).ToList();

        List<string> tempBlockedNames = [];
        
        List<VanId> storedVanIds;
        
        using (var scope = _scopeFactory.CreateScope())
        {
            var trelloContext = scope.ServiceProvider.GetRequiredService<TrelloContext>();
            
            storedVanIds = trelloContext.VanIds.ToList();
        }

        foreach (Card lineMoveCard in lineMoveBoardCards)
        {
            
            if (ModelNameMatcher.TryGetSingleName(lineMoveCard.Name, out var model, out var number, out string? formattedName))
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

                VanId?  vanId = storedVanIds.SingleOrDefault(x => x.VanName == formattedName);
                string? idString;
                string? urlString;
                
                if(vanId is not null && vanId.Blocked)
                    continue;

                if (vanId is null || string.IsNullOrEmpty(vanId.Id) || string.IsNullOrEmpty(vanId.Url))
                {
                    TimeSpan lastUpdated = DateTimeOffset.UtcNow - lineMoveCard.LastActivity.UtcDateTime;
                    var      search      = await TrySearchForVanId(formattedName, lastUpdated);

                    if (!search.Success)
                        continue;

                    else
                    {
                        idString = search.Object.Id;
                        urlString = search.Object.Url;
                    }
                }
                else
                {
                    idString = vanId.Id;
                    urlString = vanId.Url;
                }

                List<(DateTimeOffset moveDate, Location location)> locationHistory = [];
                
                foreach (var moveAction in lineMoveActions.Where(x => x.Data.Card.Id == lineMoveCard.Id))
                {
                    List list;

                    if (lists.Count(x => x.Id == moveAction.Data.ListAfter.Id) == 1)
                        list = lists.Single(x => x.Id == moveAction.Data.ListAfter.Id);

                    else
                        continue;

                    string listName = list.Name;

                    if (listName == "SCHEDULED VANS (50x)" || listName == "SCHEDULED EXPO VANS")
                    {
                        if(locationHistory.All(x => x.location != LocationFactory.PreProduction))
                            locationHistory.Add((moveAction.Date, LocationFactory.PreProduction));
                        
                        continue;
                    }

                    if (listName == "OUTSIDE - Carpark GEN2 WIP" || listName == "OUTSIDE - Carpark GEN2 Ready For Transport")
                    {
                        if(locationHistory.All(x => x.location != LocationFactory.PostProduction))
                            locationHistory.Add((moveAction.Date, LocationFactory.PostProduction));
                        
                        continue;
                    }

                    if (listName == "OUTSIDE - Carpark EXPO Ready For Transport" || listName == "OUTSIDE - Carpark EXPO - WIP")
                    {
                        if(locationHistory.All(x => x.location != LocationFactory.PostProduction))
                            locationHistory.Add((moveAction.Date, LocationFactory.PostProduction));
                        
                        continue;
                    }

                    var location = LocationFactory.GetLocationFromCustomName(model.ProductionLine, listName);
                    
                    if(location is null)
                        continue;
                    
                    locationHistory.Add((moveAction.Date, location));
                }

                Vans.TryAdd(formattedName, new SalesProductionInfo()
                {
                    Number = number, 
                    Model = model, 
                    Url = urlString, 
                    Id = idString,
                    LocationInfo = new LocationInfo(locationHistory)
                });
                
                Log.Logger.Debug("New van information added");
            }
            else
            {
                Log.Logger.Debug("Does not represent a van, ignoring.");
            }
        }

        Log.Logger.Information("{vanCount} Vans added to the production service. Adding handover dates...", Vans.Count);

        foreach (Card card in prohoCards)
        {
            if (ModelNameMatcher.TryGetSingleName(card.Name, out string? formattedName))
            {
                if (!card.Due.HasValue)
                    continue;

                if (Vans.TryGetValue(formattedName, out SalesProductionInfo? value))
                {
                    List<CachedTrelloAction> actions = prohoCachedActions.Where(x => x.CardId == card.Id && x.DueDate.HasValue).ToList();

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
            var vansInLine = Vans.Where(x => x.Value.Model.ProductionLine == productionLine).Select(x => x.Value).ToList();
                
            int prepCount = vansInLine.Count(x =>
                x.LocationInfo.CurrentLocation.Type is ProductionLocationType.Prep && x.HandoverState is HandoverState.HandedOver);
                
            int prodCount = vansInLine.Count(x =>
                x.LocationInfo.CurrentLocation.Type is ProductionLocationType.Bay or ProductionLocationType.Module or ProductionLocationType.Subassembly);
                
            int finishingCount = vansInLine.Count(x =>
                x.LocationInfo.CurrentLocation.Type is ProductionLocationType.Finishing && x.HandoverState is not HandoverState.HandedOver);
            
            int handoverDueCount = vansInLine.Count(x =>
                x.HandoverDate < DateTimeOffset.Now && x.HandoverState is HandoverState.UnhandedOver);
            
            int handedOverCount = vansInLine.Count(x =>
                x.HandoverState is HandoverState.HandedOver);
            
            Log.Logger.Information("{line}: Prep: {prepCount} - In Production: {prodCount} - In Finishing: {finishingCount} - Over Due: {overdueCount} - Handed Over: {handoverCount}",
                                   productionLine.Name, prepCount, prodCount, finishingCount, handoverDueCount ,handedOverCount);
        }
    }

    protected override async Task<SalesProductionInfo> _loadVanFromSourceAsync(SalesProductionInfo van)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var trelloContext = scope.ServiceProvider.GetRequiredService<TrelloContext>();

            GetCardOptions getCardOptions =
                new GetCardOptions
                {
                    IncludeChecklists       = true,
                    ChecklistFields         = ChecklistFields.All,
                    IncludeList             = true,
                    IncludeBoard            = true,
                    BoardFields             = new BoardFields(BoardFieldsType.Name),
                    IncludeCustomFieldItems = true,
                    IncludeAttachments      = GetCardOptionsIncludeAttachments.True,
                    CardFields =
                        new CardFields(
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

                    var oldId = trelloContext.VanIds.Single(x => x.VanName == van.Name);

                    trelloContext.VanIds.Entry(oldId).State = EntityState.Deleted;
                    trelloContext.SaveChanges();

                    var newId = await TrySearchForVanId(van.Name);

                    if (!newId.Success)
                        throw new Exception("Board could not be found");

                    van.Id  = newId.Object.Id;
                    van.Url = newId.Object.Url;
                    cards   = await TrelloClient.GetCardsOnBoardAsync(newId.Object.Id, getCardOptions);
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
                var cardFields  = customFields.Where(x => x.ModelId == van.Id).ToList();

                CardType cardType = TrelloUtils.GetCardType(card.Name, card.List.Name);

                switch (cardType)
                {
                    case CardType.JobCard:
                        var position = LocationFactory.GetLocationFromCustomName(van.Model.ProductionLine, card.List.Name);
                        if (position is not null)
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
    }


    private async Task<List<CachedTrelloAction>> GetTrelloActionsWithCache(string id, List<string>? actionFilters = null)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var trelloContext = scope.ServiceProvider.GetRequiredService<TrelloContext>();

            GetActionsOptions getActionsOptions =
                new GetActionsOptions()
                {
                    Limit = 1000, Filter = actionFilters ?? TrelloActionFilters
                };

            IEnumerable<CachedTrelloAction> cachedActions = trelloContext.Actions.Where(x => x.BoardId == id).ToList();

            List<TrelloAction> actionsToCache = [];

            if (cachedActions.Any())
                getActionsOptions.Since = cachedActions.MaxBy(x => x.DateOffset)!.ActionId;

            List<TrelloAction> newActions = await TrelloClient.GetActionsOfBoardAsync(id, getActionsOptions);

            actionsToCache.AddRange(newActions);

            while (newActions.Count == 1000)
            {
                string lastId = newActions.Last().Id;

                if (!cachedActions.Any())
                {
                    getActionsOptions.Before = lastId;
                }

                else
                {
                    getActionsOptions.Before = lastId;
                    getActionsOptions.Since  = cachedActions.Last().ActionId;
                }

                newActions = await TrelloClient.GetActionsOfBoardAsync(id, getActionsOptions);

                actionsToCache.AddRange(newActions);
            }

            if (actionsToCache.Count > 0)
            {
                List<CachedTrelloAction> newCachedActions = actionsToCache.ToCachedTrelloActions().ToList();

                await trelloContext.Actions.AddRangeAsync(newCachedActions);
                await trelloContext.SaveChangesAsync();

                cachedActions = newCachedActions.Concat(cachedActions);
            }

            return cachedActions.ToList();
        }
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
    
    private async Task<TryObject<VanId>> TrySearchForVanId(string name, TimeSpan? age = null)
    {
        using (var scope = _scopeFactory.CreateScope())
        {
            var trelloContext = scope.ServiceProvider.GetRequiredService<TrelloContext>();

            if (TrelloClient is null)
                throw new Exception("Trello Client has not been initialized.");

            VanId? vanId = await trelloContext.VanIds.SingleOrDefaultAsync(x => x.VanName == name);

            if (vanId is null)
            {
                vanId = new VanId() { VanName = name };
                await trelloContext.VanIds.AddAsync(vanId);
            }
            else
            {
                if (vanId.Blocked)
                    return new TryObject<VanId>(false, null);

                if (!string.IsNullOrEmpty(vanId.Id) && !string.IsNullOrEmpty(vanId.Url))
                    return new TryObject<VanId>(true, null);
            }

            SearchRequest searchRequest =
                new SearchRequest(name)
                {
                    SearchCards = false, BoardFields = new SearchRequestBoardFields("name", "closed", "url", "shortUrl")
                };

            SearchResult searchResults = await TrelloClient.SearchAsync(searchRequest);

            List<Board> results = searchResults.Boards.Where(x => !x.Closed).ToList();

            if (results.Count > 1)
            {
                Log.Logger.Error("Multiple Boards found for van {name}, not adding to cache - {urlList}", name, string.Join(", ", results.Select(x => $"https://trello.com/b/{x.Id}/")));

                return new TryObject<VanId>(false, null);
            }

            if (!results.Any())
            {
                if (age.HasValue && age > TimeSpan.FromDays(90))
                {
                    vanId.Blocked = true;

                    Log.Logger.Warning("No trello search result for {name}, blocking van from future searches", name);
                }
                else
                    Log.Logger.Warning("No trello search result for {name}", name);


                await trelloContext.SaveChangesAsync();
                return new TryObject<VanId>(false, null);
            }

            vanId.Id  = results.First().Id;
            vanId.Url = results.First().Url;

            await trelloContext.SaveChangesAsync();

            return new TryObject<VanId>(true, vanId);
        }
    }
}