﻿using System.Diagnostics.CodeAnalysis;
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
using ZoneRV.DataAccess.Data;
using ZoneRV.DataAccess.Models;
using ZoneRV.Extensions;
using ZoneRV.Models;
using ZoneRV.Models.Enums;
using ZoneRV.Services.Production;
using TrelloActionData = ZoneRV.DataAccess.Data.TrelloActionData;

namespace ZoneRV.Services.TrelloProduction;

public partial class TrelloService : IProductionService
{
    public VanIdData VanIdData { get; set; } 
    public TrelloActionData TrelloActionData { get; set; } 
    public static List<string> TrelloActionFilters => ["commentCard", "updateCustomFieldItem", "createCard", "updateCheckItemStateOnCard"];
    
    private TrelloClient TrelloClient { get; init; }
    private string TrelloApiKey { get; }
    private string TrelloUserToken { get; }
    private string LineMoveBoardId { get; }
    private string CCDashboardId { get; }
    
    public TrelloService(IConfiguration configuration, VanIdData vanIdData, TrelloActionData trelloActionData) : base(configuration)
    {
        VanIdData = vanIdData;
        TrelloActionData = trelloActionData;
        
        TrelloApiKey = configuration["TrelloApiKey"] ??
                       throw new ArgumentNullException(nameof(TrelloApiKey), "Trello api key required");
        
        TrelloUserToken = configuration["TrelloUserToken"] ??
                       throw new ArgumentNullException(nameof(TrelloUserToken), "Trello user token required");
        
        LineMoveBoardId = configuration["LineMoveBoardId"] ??
                          throw new ArgumentNullException(nameof(LineMoveBoardId), "Line move board id required");
        
        CCDashboardId = configuration["CCDashboardId"] ??
                        throw new ArgumentNullException(nameof(CCDashboardId), "CC Dashboard board id required");
        
        var clientOptions = new TrelloClientOptions
        {
            AllowDeleteOfBoards = false,
            AllowDeleteOfOrganizations = false,
            MaxRetryCountForTokenLimitExceeded = 3
        };

        TrelloClient = new TrelloClient(TrelloApiKey, TrelloUserToken, clientOptions);
    }

    protected override async Task InitialiseService(IConfiguration configuration)
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
        List<TrelloAction> lineMoveActions    = await TrelloClient.GetActionsOfBoardAsync(LineMoveBoardId, ["updateCard:idList"], 1000);
        lineMoveActions.AddRange(await TrelloClient.GetActionsOfBoardAsync(LineMoveBoardId, ["updateCard:idList"], 1000, before: lineMoveActions.MinBy(x => x.Date)!.Id));
        
        List<Card>         ccDashboardCards   = await TrelloClient.GetCardsOnBoardAsync(CCDashboardId, new GetCardOptions());

        var cachedActions = await GetTrelloActions(CCDashboardId);

        List<List>? lists = await TrelloClient.GetListsOnBoardAsync(LineMoveBoardId);

        lineMoveBoardCards = lineMoveBoardCards
                             .Where(c 
                                 => Enum.GetNames<VanModel>()
                                     .Any(vm 
                                        => c.Name.ToLower().Contains(vm.ToLower()) 
                                    )).ToList();
        
        ccDashboardCards = ccDashboardCards
                            .Where(c 
                                => Enum.GetNames<VanModel>()
                                    .Any(vm 
                                        => c.Name.ToLower().Contains(vm.ToLower()) 
                                    )).ToList();

        List<string> tempBlockedNames = [];

        IEnumerable<VanID> storedVanIds = await VanIdData.GetIds();
        List<VanID> storedVanIdList = storedVanIds.ToList();

        foreach (Card card in lineMoveBoardCards)
        {
            
            if (Utils.TryGetVanName(card.Name, out _, out string? formattedName))
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

                VanID? vanId = storedVanIdList.SingleOrDefault(x => x.VanName == formattedName);
                string? idString;
                string? urlString;
                
                if(vanId is not null && vanId.Blocked)
                    continue;

                if (vanId is null || string.IsNullOrEmpty(vanId.VanId) || string.IsNullOrEmpty(vanId.Url))
                {
                    TimeSpan lastUpdated = DateTimeOffset.UtcNow - card.LastActivity.UtcDateTime;
                    (bool boardfound, VanID? vanId) search = await TrySearchForVanId(formattedName, lastUpdated);

                    if (!search.boardfound || search.vanId is null)
                        continue;

                    else
                    {
                        idString = search.vanId.VanId;
                        urlString = search.vanId.Url;
                    }
                }
                else
                {
                    idString = vanId.VanId;
                    urlString = vanId.Url;
                }

                //List<(DateTimeOffset date, IProductionPosition)> positionHistory = lineMoveActions.Where(x => x.Data.Card.Id == card.Id).ToPositionHistory(lists);

                //ProductionVans.Add(formattedName,
                 //                  new VanProductionInfo(idString, formattedName, urlString, positionHistory));
                
                Log.Logger.Debug("New van information added");
            }
            else
            {
                Log.Logger.Debug("Does not represent a van, ignoring.");
            }
        }

        //Log.Logger.Information("{vanCount} Vans added to the production service. Adding handover dates...", ProductionVans.Count);

        foreach (Card card in ccDashboardCards)
        {
            
            if (Utils.TryGetVanName(card.Name, out _, out string? formattedName))
            {
                if (!card.Due.HasValue)
                    continue;

                if (Vans.TryGetValue(formattedName, out VanProductionInfo? value))
                {
                    IEnumerable<CachedTrelloAction> actions = cachedActions.Where(x => x.CardId == card.Id && x.DueDate.HasValue);

                    foreach (CachedTrelloAction action in actions.OrderBy(x => x.DateOffset))
                    {
                        value.AddHandoverHistory(action.DateOffset, action.DueDate!.Value);
                    }

                    if (card.Due.HasValue)
                    {
                        value.HandoverState =
                            card.DueComplete ? HandoverState.HandedOver : HandoverState.UnhandedOver;
                        
                        var dueUpdatedAction = actions.LastOrDefault(x => x.DueDate.HasValue);
                        
                        //if(dueUpdatedAction is not null)
                            //value.han = dueUpdatedAction.DateOffset;
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

        int gen2PreProductionCount = Vans.Count(x => x.Key.GetModel()!.Value.GetProductionLine() is ProductionLine.Gen2 && x.Value.LocationInfo.CurrentLocation.Type is ProductionLocationType.Prep);
        int expoPreProductionCount = Vans.Count(x => x.Key.GetModel()!.Value.GetProductionLine() is ProductionLine.Expo && x.Value.LocationInfo.CurrentLocation.Type is ProductionLocationType.Prep);

        int gen2ProductionCount = Vans.Count(x => x.Value.LocationInfo.CurrentLocation.ProductionLine is ProductionLine.Gen2);
        int expoProductionCount = Vans.Count(x => x.Value.LocationInfo.CurrentLocation.ProductionLine is ProductionLine.Expo);

        int gen2PostProductionCount = Vans.Count(x => x.Key.GetModel()!.Value.GetProductionLine() is ProductionLine.Gen2 && x.Value.LocationInfo.CurrentLocation.Type is ProductionLocationType.Bay or ProductionLocationType.Module);
        int expoPostProductionCount = Vans.Count(x => x.Key.GetModel()!.Value.GetProductionLine() is ProductionLine.Expo && x.Value.LocationInfo.CurrentLocation.Type is ProductionLocationType.Bay or ProductionLocationType.Module);

        int gen2PostProductionUnhandedOverCount = Vans.Count(x => x.Key.GetModel()!.Value.GetProductionLine() is ProductionLine.Gen2 && x.Value.LocationInfo.CurrentLocation.Type is ProductionLocationType.Finishing);
        int expoPostProductionUnhandedOverCount = Vans.Count(x => x.Key.GetModel()!.Value.GetProductionLine() is ProductionLine.Expo && x.Value.LocationInfo.CurrentLocation.Type is ProductionLocationType.Finishing);
        
        Log.Logger.Information("Gen 2: Pre-Production:{pre} - In Production:{prod} - Post Production:{post} ({yetToHandover} unhanded over)", gen2PreProductionCount, gen2ProductionCount, gen2PostProductionCount, gen2PostProductionUnhandedOverCount);
        Log.Logger.Information("EXPO: Pre-Production:{pre} - In Production:{prod} - Post Production:{post} ({yetToHandover} unhanded over)", expoPreProductionCount, expoProductionCount, expoPostProductionCount, expoPostProductionUnhandedOverCount);
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

                (bool boardfound, VanID? vanId) newId = await TrySearchForVanId(van.Name);

                if (!newId.boardfound || newId.vanId is null || string.IsNullOrEmpty(newId.vanId.VanId) || string.IsNullOrEmpty(newId.vanId.Url))
                    throw new Exception("Board could not be found");
                
                van.Id = newId.vanId.VanId;
                van.Url = newId.vanId.Url;
                cards = await TrelloClient.GetCardsOnBoardAsync(newId.vanId.VanId, getCardOptions);
            }
            else
            {
                throw;
            }
        }
        
        List<CustomField> customFields = await TrelloClient.GetCustomFieldsOnBoardAsync(van.Id);

        var cachedActions = await GetTrelloActions(van.Id!);

        foreach (var card in cards)
        {
            var cardActions = cachedActions.Where(x => x.BoardId == van.Id).ToList();
            var cardFields = customFields.Where(x => x.ModelId == van.Id).ToList();

            CardType cardType = TrelloUtils.GetCardType(card.Name, card.List.Name);

            switch (cardType)
            {
                case CardType.JobCard:
                    CreateJobCard(van, card.ToJobCardInfo(cardActions, cardFields), TrelloUtils.ToAreaOfOrigin(card, cardFields), TrelloUtils.GetProductionLocation());
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

    private async Task<IEnumerable<CachedTrelloAction>> GetTrelloActions(string id)
    {
        GetActionsOptions getActionsOptions = new GetActionsOptions()
        {
            Limit = 1000,
            Filter = TrelloActionFilters
        };
        
        IEnumerable<CachedTrelloAction> cachedActions = await TrelloActionData.GetActions(id);
        cachedActions = cachedActions.ToList();
        
        List<TrelloAction> actionsToCache = [];

        if (cachedActions.Count() == 0)
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

        return cachedActions;
    }
    
    private async Task<(bool boardfound, VanID? vanId)> TrySearchForVanId(string name, TimeSpan? age = null)
    {
        if (TrelloClient is null)
            throw new Exception("Trello Client has not been initialized.");

        VanID? vanId = await VanIdData.GetId(name);

        if (vanId is null)
        {
            vanId = new VanID(name);
            await VanIdData.InsertVanId(vanId);
        }
        else
        {
            if (vanId.Blocked)
                return (false, null);

            if (!string.IsNullOrEmpty(vanId.VanId) && !string.IsNullOrEmpty(vanId.Url))
                return (true, null);
        }

        SearchRequest searchRequest = new SearchRequest(name)
                                      {
                                          SearchCards = false,
                                          BoardFields = new SearchRequestBoardFields("name", "closed", "url", "shorturl")
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

        vanId.VanId = results.First().Id;
        vanId.Url = results.First().Url;
        
        await VanIdData.UpdateVanId(vanId);

        var id = await VanIdData.GetId(name);
        
        return (true, id);
    }
}