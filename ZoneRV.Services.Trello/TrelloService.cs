using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TrelloDotNet;
using TrelloDotNet.Model;
using TrelloDotNet.Model.Actions;
using TrelloDotNet.Model.Batch;
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
    
    public static List<string> TrelloActionFilters => ["commentCard", "updateCustomFieldItem", "createCard", "updateCheckItemStateOnCard"];
    
    protected override string ServiceTypeName { get => "trello"; }
    
    internal TrelloClient TrelloClient     { get; }
    private Member?      TrelloMember     { get; set; }
    private string       TrelloApiKey     { get; }
    private string       TrelloUserToken  { get; }
    private string       LineMoveBoardId  { get; }
    private string       ProHoDashboardId { get; }

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
        
        var clientOptions = new TrelloClientOptions
        {
            AllowDeleteOfBoards = false,
            AllowDeleteOfOrganizations = false,
            MaxRetryCountForTokenLimitExceeded = 3
        };

        TrelloClient = new TrelloClient(TrelloApiKey, TrelloUserToken, clientOptions);
    }


    protected override async Task SetupService(CancellationToken cancellationToken = default)
    {
        try
        {
            TrelloMember = await TrelloClient.GetTokenMemberAsync(cancellationToken);
            Log.Logger.Information("Trello Successfully connected as {member}", TrelloMember.Username);
        }
        catch (Exception ex)
        {
            Log.Logger.Fatal(ex, "Trello client could not connect");

            throw;
        }
    }

    protected override async Task LoadUsers(CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(TrelloMember);
        
        var organisations = await TrelloClient.GetOrganizationsForMemberAsync(TrelloMember.Id, cancellationToken);
        
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

            List<Member> members = await TrelloClient.GetMembersOfOrganizationAsync(organization.Id, getMemberOptions, cancellationToken);

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

    protected override async Task LoadProductionInfo(CancellationToken cancellationToken = default)
    {
        List<Card>         lineMoveBoardCards = await TrelloClient.GetCardsOnBoardAsync(LineMoveBoardId, new GetCardOptions() { IncludeList = true }, cancellationToken);
        List<TrelloAction> lineMoveActions    = await TrelloClient.GetActionsOfBoardAsync(LineMoveBoardId, new GetActionsOptions() { Limit  = 1000, Filter = ["updateCard:idList"] }, cancellationToken);

        List<Card> prohoCards         = await TrelloClient.GetCardsOnBoardAsync(ProHoDashboardId, new GetCardOptions(), cancellationToken);
        var        prohoCachedActions = await GetTrelloActionsWithCache(ProHoDashboardId, cancellationToken, ["updateCard"]);

        List<List>? lists = await TrelloClient.GetListsOnBoardAsync(LineMoveBoardId, cancellationToken);

        List<string> modelPrefixes = ProductionLines.SelectMany(x => x.Models.Select(y => y.Prefix.ToLower())).ToList();

        lineMoveBoardCards = lineMoveBoardCards
            .Where(c => modelPrefixes.Any(vm => c.Name.ToLower().Contains(vm.ToLower()))).ToList();

        prohoCards = prohoCards
            .Where(c => modelPrefixes.Any(vm => c.Name.ToLower().Contains(vm.ToLower()))).ToList();

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

                if (SalesOrders.ContainsKey(formattedName))
                {
                    SalesOrders.TryRemove(formattedName, out _);
                    tempBlockedNames.Add(formattedName);

                    Log.Logger.Error("{name} found at least twice in line move board. Ignoring both until issue is resolved.", formattedName);
                    continue;
                }

                VanId? vanId = storedVanIds.SingleOrDefault(x => x.VanName == formattedName);
                string? idString;
                string? urlString;

                if (vanId is not null && vanId.Blocked) continue;

                if (vanId is null || string.IsNullOrEmpty(vanId.Id) || string.IsNullOrEmpty(vanId.Url))
                {
                    TimeSpan lastUpdated = DateTimeOffset.UtcNow - lineMoveCard.LastActivity.UtcDateTime;
                    var search = await TrySearchForVanId(formattedName, lastUpdated);

                    if (!search.Success) continue;

                    idString = search.Object.Id;
                    urlString = search.Object.Url;
                }
                else
                {
                    idString = vanId.Id;
                    urlString = vanId.Url;
                }

                List<(DateTimeOffset moveDate, OrderedLineLocation location)> locationHistory = [];

                foreach (var moveAction in lineMoveActions.Where(x => x.Data.Card.Id == lineMoveCard.Id))
                {
                    List list;

                    if (lists.Count(x => x.Id == moveAction.Data.ListAfter.Id) == 1)
                        list = lists.Single(x => x.Id == moveAction.Data.ListAfter.Id);
                    else
                        continue;

                    string listName = list.Name;

                    var location = LocationFactory.GetLocationFromCustomName(model.Line, listName);

                    if (location is null) 
                        continue;

                    locationHistory.Add((moveAction.Date, location));
                }

                SalesOrders.TryAdd(formattedName, new SalesOrder()
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

        Log.Logger.Information("{vanCount} Vans added to the production service. Adding handover dates...", SalesOrders.Count);

        foreach (Card card in prohoCards)
        {
            if (ModelNameMatcher.TryGetSingleName(card.Name, out string? formattedName))
            {
                if (!card.Due.HasValue) continue;

                if (SalesOrders.TryGetValue(formattedName, out SalesOrder? value))
                {
                    List<CachedTrelloAction> actions = prohoCachedActions.Where(x => x.CardId == card.Id && x.DueDate.HasValue).ToList();

                    foreach (CachedTrelloAction action in actions.OrderBy(x => x.DateOffset))
                    {
                        value.AddHandoverHistory(action.DateOffset, action.DueDate!.Value);
                    }

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
    }

    protected override async Task<SalesOrder> _loadSalesOrderFromSourceAsync(SalesOrder van, CancellationToken cancellationToken = default)
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
                cards = await TrelloClient.GetCardsOnBoardAsync(van.Id, getCardOptions, cancellationToken);
            }
            catch (Exception ex) //TODO: Handle "TrelloDotNet.Model.TrelloApiException: The requested resource was not found" exception
            {
                if (ex is TrelloApiException && ex.Message == "invalid id")
                {
                    Log.Logger.Warning("{vanName} board id no longer exists. Refreshing Cached id.", van.Name);

                    var oldId = trelloContext.VanIds.Single(x => x.VanName == van.Name);

                    trelloContext.VanIds.Entry(oldId).State = EntityState.Deleted;
                    await trelloContext.SaveChangesAsync(cancellationToken);

                    var newId = await TrySearchForVanId(van.Name);

                    if (!newId.Success)
                        throw new Exception("Board could not be found");

                    van.Id  = newId.Object.Id;
                    van.Url = newId.Object.Url;
                    cards   = await TrelloClient.GetCardsOnBoardAsync(newId.Object.Id, getCardOptions, cancellationToken);
                }
                else
                {
                    throw;
                }
            }

            List<CustomField> customFields = await TrelloClient.GetCustomFieldsOnBoardAsync(van.Id, cancellationToken);

            var cachedActions = await GetTrelloActionsWithCache(van.Id!, cancellationToken);

            foreach (var card in cards)
            {
                if (card.Name.ToLower().StartsWith("vin"))
                {
                    var regex = Regex.Match(card.Name, @"6K9CARVANSC\d{6}|6K9CARVANRC\d{6}|6K9CARVANPC\d{6}");

                    if (regex.Success)
                    {
                        van.Vin = regex.Value;
                    }

                    else
                    {
                        Log.Logger.Warning("Failed to find vin number in card {cardName} on {boardName}", card.Name, van.Name);
                    }
                    
                    continue;
                }
                
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
    
    public override async Task<ChecklistCreationInfo> GetChecklistFromSource(string checklistId)
    {
        var checklist = await TrelloClient.GetChecklistAsync(checklistId);
        var actions =  await TrelloClient.GetActionsOnCardAsync(checklist.CardId);

        return checklist.ToChecklistInfo(actions.ToCachedTrelloActions());
    }
    
    private async Task<List<CachedTrelloAction>> GetTrelloActionsWithCache(string id, CancellationToken cancellationToken = default, List<string>? actionFilters = null)
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

            List<TrelloAction> newActions = await TrelloClient.GetActionsOfBoardAsync(id, getActionsOptions, cancellationToken);

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

                newActions = await TrelloClient.GetActionsOfBoardAsync(id, getActionsOptions, cancellationToken);

                actionsToCache.AddRange(newActions);
            }

            if (actionsToCache.Count > 0)
            {
                List<CachedTrelloAction> newCachedActions = actionsToCache.ToCachedTrelloActions().ToList();

                await trelloContext.Actions.AddRangeAsync(newCachedActions, cancellationToken);
                await trelloContext.SaveChangesAsync(cancellationToken);

                cachedActions = newCachedActions.Concat(cachedActions);
            }

            return cachedActions.ToList();
        }
    }

    private async Task<List<TrelloAction>> GetTrelloActions(string id, CancellationToken cancellationToken = default, List<string>? filteredActions = null)
    {
        GetActionsOptions getActionsOptions = new GetActionsOptions()
        {
            Limit = 1000,
            Filter = filteredActions ?? TrelloActionFilters
        };
        
        List<TrelloAction> actions = [];
       
        List<TrelloAction> newActions = await TrelloClient.GetActionsOfBoardAsync(id, getActionsOptions, cancellationToken);

        actions.AddRange(newActions);
        
        while (newActions.Count == 1000)
        {
            string lastId = newActions.Last().Id;
            
            getActionsOptions.Before = lastId;
            
            newActions = await TrelloClient.GetActionsOfBoardAsync(id, getActionsOptions, cancellationToken);
            
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