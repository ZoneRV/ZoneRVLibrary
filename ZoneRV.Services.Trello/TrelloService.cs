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
    
    protected override string ServiceTypeName { get => "trello"; }
    
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
        

        LocationFactory = new LocationFactory(ServiceTypeName)
        {
            Workspaces = Workspaces!, // TODO: Check if this actually loads in correct order
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
        // Fetch all the cards and actions on the Line Move Board
        List<Card> lineMoveBoardCards = await TrelloClient.GetCardsOnBoardAsync(LineMoveBoardId, new GetCardOptions() { IncludeList = true });
        List<TrelloAction> lineMoveActions = await TrelloClient.GetActionsOfBoardAsync(LineMoveBoardId, new GetActionsOptions() { Limit = 1000, Filter = ["updateCard:idList"] });

        // Fetch all the cards and actions on the "ProHo Dashboard"
        List<Card> prohoCards = await TrelloClient.GetCardsOnBoardAsync(ProHoDashboardId, new GetCardOptions());
        var prohoCachedActions = await GetTrelloActionsWithCache(ProHoDashboardId, ["updateCard"]);

        // Fetch the lists on the "Line Move Board"
        List<List>? lists = await TrelloClient.GetListsOnBoardAsync(LineMoveBoardId);

        // Build a list of model prefixes used to match card names
        List<string> modelPrefixes = ProductionLines.SelectMany(x => x.Models.Select(y => y.Prefix.ToLower())).ToList();

        // Filter cards on the Line Move and ProHo Boards to match relevant vehicle model prefixes
        lineMoveBoardCards = lineMoveBoardCards
            .Where(c => modelPrefixes.Any(vm => c.Name.ToLower().Contains(vm.ToLower()))).ToList();

        prohoCards = prohoCards
            .Where(c => modelPrefixes.Any(vm => c.Name.ToLower().Contains(vm.ToLower()))).ToList();

        // Temporary list to store duplicate (or blocked) formatted names to avoid reprocessing and ambiguity between cards with same names
        List<string> tempBlockedNames = [];

        // Retrieve stored van information from the database
        List<VanId> storedVanIds;

        using (var scope = _scopeFactory.CreateScope())
        {
            var trelloContext = scope.ServiceProvider.GetRequiredService<TrelloContext>();
            
            storedVanIds = trelloContext.VanIds.ToList();
        }

        // Iterate through every card in the filtered "Line Move Board" cards
        foreach (Card lineMoveCard in lineMoveBoardCards)
        {
            // Attempt to extract the model, number, and formatted name from the card name
            if (ModelNameMatcher.TryGetSingleName(lineMoveCard.Name, out var model, out var number, out string? formattedName))
            {
                // Skip processing if the card's formatted name is already in the blocked list
                if (tempBlockedNames.Contains(formattedName))
                    continue;

                // If the van is already in the dictionary, handle duplicates
                if (Vans.ContainsKey(formattedName))
                {
                    // Remove the duplicate van and block its processing
                    Vans.TryRemove(formattedName, out _);
                    tempBlockedNames.Add(formattedName);

                    Log.Logger.Error("{name} found at least twice in line move board. Ignoring both until issue is resolved.", formattedName);
                    continue;
                }

                // Retrieve the van ID and associated URL if it exists in the database
                VanId? vanId = storedVanIds.SingleOrDefault(x => x.VanName == formattedName);
                string? idString;
                string? urlString;

                // Skip blocked van IDs
                if (vanId is not null && vanId.Blocked) continue;

                // If the van doesn't exist or its ID/URL is missing, attempt to search for it
                if (vanId is null || string.IsNullOrEmpty(vanId.Id) || string.IsNullOrEmpty(vanId.Url))
                {
                    TimeSpan lastUpdated = DateTimeOffset.UtcNow - lineMoveCard.LastActivity.UtcDateTime;
                    var search = await TrySearchForVanId(formattedName, lastUpdated);

                    // Skip further processing if the van search failed
                    if (!search.Success) continue;

                    // Otherwise, populate ID and URL from the search results
                    idString = search.Object.Id;
                    urlString = search.Object.Url;
                }
                else
                {
                    // Use the existing ID and URL in the database
                    idString = vanId.Id;
                    urlString = vanId.Url;
                }

                // Track location history for the current van
                List<(DateTimeOffset moveDate, OrderedLineLocation location)> locationHistory = [];

                // Process move actions for the current card to build location history
                foreach (var moveAction in lineMoveActions.Where(x => x.Data.Card.Id == lineMoveCard.Id))
                {
                    List list;

                    // Find the list after the card was moved
                    if (lists.Count(x => x.Id == moveAction.Data.ListAfter.Id) == 1)
                        list = lists.Single(x => x.Id == moveAction.Data.ListAfter.Id);
                    else
                        continue;

                    string listName = list.Name;

                    // Attempt to resolve a custom location for the current production line based on the list name
                    var location = LocationFactory.GetLocationFromCustomName(model.Line, listName);

                    if (location is null) 
                        continue;

                    locationHistory.Add((moveAction.Date, location));
                }

                // Add the new van information to the dictionary with its location history
                Vans.TryAdd(formattedName, new SalesOrder()
                {
                    Number = number,
                    Model = model,
                    Url = urlString,
                    Id = idString,
                    LocationInfo = new LocationInfo(model.Line, locationHistory)
                });

                Log.Logger.Debug("New van information added");
            }
            else
            {
                Log.Logger.Debug("Does not represent a van, ignoring.");
            }
        }

        // Log the total number of vans added to the production service
        Log.Logger.Information("{vanCount} Vans added to the production service. Adding handover dates...", Vans.Count);

        // Process "ProHo Dashboard" cards to add handover information to vans
        foreach (Card card in prohoCards)
        {
            // Attempt to extract the van's name from the card's name
            if (ModelNameMatcher.TryGetSingleName(card.Name, out string? formattedName))
            {
                // Skip cards without a due date
                if (!card.Due.HasValue) continue;

                // Find and update the corresponding van's handover information
                if (Vans.TryGetValue(formattedName, out SalesOrder? value))
                {
                    // Get actions with due dates for the current card
                    List<CachedTrelloAction> actions = prohoCachedActions.Where(x => x.CardId == card.Id && x.DueDate.HasValue).ToList();

                    foreach (CachedTrelloAction action in actions.OrderBy(x => x.DateOffset))
                    {
                        value.AddHandoverHistory(action.DateOffset, action.DueDate!.Value);
                    }

                    // Update the handover state based on the card's due completion status
                    if (card.Due.HasValue)
                    {
                        value.HandoverState = card.DueComplete ? HandoverState.HandedOver : HandoverState.UnhandedOver;

                        var dueUpdatedAction = actions.LastOrDefault(x => x.DueDate.HasValue);
                        if (dueUpdatedAction is not null)
                            value.HandoverStateLastUpdated = dueUpdatedAction.DateOffset;
                    }

                    Log.Logger.Debug(
                        "Added {handover} to {vanName} ({handoverStat})",
                        card.Due.Value.LocalDateTime.Date.ToString("dd/MM/yy"),
                        value.Name,
                        value.HandoverState
                    );
                }
            }
            else
            {
                Log.Logger.Debug("Does not represent a van, ignoring.");
            }
        }

        // Log the count of vans with updated handover dates
        Log.Logger.Information("{vanCount} handover dates added", Vans.Count(x => x.Value.RedlineDate.HasValue));

        // Generate logs for the current production statistics across all production lines
        foreach (var productionLine in ProductionLines)
        {
            var vansInLine = Vans.Where(x => x.Value.Model.Line == productionLine).Select(x => x.Value).ToList();

            int prepCount = vansInLine.Count(x =>
                x.LocationInfo.CurrentLocation is not null && x.LocationInfo.CurrentLocation.Location.LocationType is ProductionLocationType.Prep && x.HandoverState is HandoverState.HandedOver);

            int prodCount = vansInLine.Count(x =>
                x.LocationInfo.CurrentLocation is not null && x.LocationInfo.CurrentLocation.Location.LocationType is ProductionLocationType.Bay or ProductionLocationType.Module or ProductionLocationType.Subassembly);

            int finishingCount = vansInLine.Count(x =>
                x.LocationInfo.CurrentLocation is not null && x.LocationInfo.CurrentLocation.Location.LocationType is ProductionLocationType.Finishing && x.HandoverState is not HandoverState.HandedOver);

            int handoverDueCount = vansInLine.Count(x =>
                x.RedlineDate < DateTimeOffset.Now && x.HandoverState is HandoverState.UnhandedOver);

            int handedOverCount = vansInLine.Count(x =>
                x.HandoverState is HandoverState.HandedOver);

            Log.Logger.Information(
                "{line}: Prep: {prepCount} - In Production: {prodCount} - In Finishing: {finishingCount} - Over Due: {overdueCount} - Handed Over: {handoverCount}",
                productionLine.Name, prepCount, prodCount, finishingCount, handoverDueCount, handedOverCount);
        }
    }

    protected override async Task<SalesOrder> _loadVanFromSourceAsync(SalesOrder van)
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
                var cardActions = cachedActions.Where(x => x.CardId == card.Id).ToList();
                var cardFields  = customFields.Where(x => x.ModelId == van.Id).ToList();

                CardType     cardType = TrelloUtils.GetCardType(card, cardFields);
                AreaOfOrigin? area    = TrelloUtils.ToAreaOfOrigin(card, cardFields, AreaOfOrigins);

                switch (cardType)
                {
                    case CardType.JobCard:
                        var position = LocationFactory.GetLocationFromCustomName(van.Model.Line, card.List.Name);
                        if (position is not null && area is not null)
                            BuildJobCard(van, card.ToJobCardInfo(cardActions, cardFields), area, position);
                        break;

                    case CardType.RedCard:
                        BuildRedCard(van, card.ToRedCardInfo(cardActions, cardFields), area);
                        break;

                    case CardType.YellowCard:
                        BuildYellowCard(van, card.ToYellowCardInfo(cardActions, cardFields), area);
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