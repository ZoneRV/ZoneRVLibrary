namespace ZoneRV.Models.Van;

public abstract class Card
{
    public Card(VanProductionInfo van, CardInfo info, AreaOfOrigin areaOfOrigin)
    {
        Van = van;
        AreaOfOrigin = areaOfOrigin;
        Id = info.Id;
        Name = info.Name;
        CardStatus = info.CardStatus;
        Url = info.Url;
        CardStatusLastUpdated = info.CardStatusLastUpdated;
    }
    
    public string Name { get; set; }
    public string Id { get; init; }
    public string Url { get; init; }
    
    public string BoardId => Van.Id;
    public VanProductionInfo Van { get; init; }
    
    public AreaOfOrigin AreaOfOrigin { get; set; }

    public List<Checklist> Checklists { get; init; } = [];

    public List<Comment> Comments { get; init; } = [];
    
    public List<Attachment> Attachments { get; init; } = [];
    
    
    [ZoneRVJsonIgnore(JsonIgnoreType.Both)] public int TotalChecks => AllChecks.Count();
    [ZoneRVJsonIgnore(JsonIgnoreType.Both)] public int CompletedCheckCount => AllChecks.Count(x => x.IsChecked);
    [ZoneRVJsonIgnore(JsonIgnoreType.Both)] public int UncompletedCheckCount => AllChecks.Count(x => !x.IsChecked);
    
    [ZoneRVJsonIgnore(JsonIgnoreType.Both)] public IEnumerable<Check> AllChecks => Checklists.SelectMany(x => x.Checks);

    
    [ZoneRVJsonIgnore(JsonIgnoreType.Api)] private CardStatus _cardStatus;
    [ZoneRVJsonIgnore(JsonIgnoreType.Api)] private DateTimeOffset? _cardStatusLastUpdated;
    
    [ZoneRVJsonIgnore(JsonIgnoreType.Cache)] public CardStatus CardStatus
    {
        get
        {
            var completionRate = GetCompletionRate();
            
            if ( completionRate > .999f)
                return CardStatus.Completed;

            if (_cardStatus is CardStatus.UnableToComplete)
                return _cardStatus;

            if (_cardStatus is CardStatus.InProgress || completionRate is > 0f and < 1f)
                return CardStatus.InProgress;

            return _cardStatus;
        }

        init => _cardStatus = value;
    }
    
    public DateTimeOffset? CardStatusLastUpdated
    {
        get => AllChecks
            .Select(x => x.LastModified)
            .Append(_cardStatusLastUpdated)
            .Max();
        
        init => _cardStatusLastUpdated = value;
    }

    public void UpdateCardStatus(CardStatus status, DateTimeOffset? timeUpdated)
    {
        _cardStatus = status;
        _cardStatusLastUpdated = timeUpdated;
    }
    
    public float GetCompletionRate()
    {
        if (_cardStatus == CardStatus.Completed)
            return 1f;
            
        if (TotalChecks == 0)
        {
            return 0f;
        }

        else
            return (float)CompletedCheckCount / (float)TotalChecks;
    }
}


public abstract class CardInfo
{
    public required CardStatus CardStatus { get; init; }
    public required string Name { get; init; }
    public required string Id { get; init; }
    public required string Url { get; init; }
    public required DateTimeOffset? CardStatusLastUpdated { get; init; }
    public IEnumerable<ChecklistInfo> ChecklistInfos { get; init; } = [];
    public IEnumerable<CommentInfo> CommentInfos { get; init; } = [];
    public IEnumerable<AttachmentInfo> AttachmentInfos { get; init; } = [];
}