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
using ZoneRV.Models;
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
    
    public TrelloService(IConfiguration configuration, VanIdData vanIdData, TrelloActionData trelloActionData) : base(configuration)
    {
        VanIdData = vanIdData;
        TrelloActionData = trelloActionData;
        
        TrelloApiKey = configuration["TrelloApiKey"] ??
                       throw new ArgumentNullException(nameof(TrelloApiKey), "Trello api key required");
        
        TrelloUserToken = configuration["TrelloUserToken"] ??
                       throw new ArgumentNullException(nameof(TrelloUserToken), "Trello user token required");
        
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

        GetActionsOptions getActionsOptions = new GetActionsOptions()
        {
            Limit = 1000,
            Filter = TrelloActionFilters
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
        
        IEnumerable<CachedTrelloAction> cachedActions = await TrelloActionData.GetActions(van.Id);
        cachedActions = cachedActions.ToList();
        
        List<TrelloAction> actionsToCache = [];

        if (cachedActions.Count() == 0)
            getActionsOptions.Since = cachedActions.Last().ActionId;
       
        List<TrelloAction> newActions = await TrelloClient.GetActionsOfBoardAsync(van.Id, getActionsOptions);

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
            
            newActions = await TrelloClient.GetActionsOfBoardAsync(van.Id, getActionsOptions);
            
            actionsToCache.AddRange(newActions);
        }

        if (actionsToCache.Count > 0)
        {
            var returnedActions = await TrelloActionData.InsertTrelloActions(actionsToCache.ToCachedTrelloActions());

            cachedActions = returnedActions.Concat(cachedActions);
        }

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