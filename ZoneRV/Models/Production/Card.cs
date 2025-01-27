namespace ZoneRV.Models.Production;

/// <summary>
/// Represents a card in the production line for a sales order containing information about its state,
/// associated production data, area of origin, and other related metadata.
/// It serves as an abstract base class for specific card types.
/// </summary>
public abstract class Card
{
    public Card(SalesProductionInfo van, CardCreationInfo info, AreaOfOrigin? areaOfOrigin)
    {
        ProductionInfo = van;
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
    
    public string BoardId => ProductionInfo.Id!; // Impossible for a van to have cards without an Id
    public SalesProductionInfo ProductionInfo { get; init; }
    
    public AreaOfOrigin? AreaOfOrigin { get; set; }

    public List<Checklist> Checklists { get; init; } = [];

    public List<Comment> Comments { get; init; } = [];
    
    public List<Attachment> Attachments { get; init; } = [];
    
    
    public int TotalChecks => AllChecks.Count();
    public int CompletedCheckCount => AllChecks.Count(x => x.IsChecked);
    public int UncompletedCheckCount => AllChecks.Count(x => !x.IsChecked);
    
    public IEnumerable<Check> AllChecks => Checklists.SelectMany(x => x.Checks);

    
    private CardStatus _cardStatus;
    private DateTimeOffset? _cardStatusLastUpdated;
    
    public CardStatus CardStatus
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

/// <summary>
/// Represents the foundational data required to create a card within the production system.
/// It includes essential information such as the card's status, identification, name, URL,
/// timestamps related to status updates, and metadata about associated items like checklists, comments, and attachments.
/// Serves as a base or shared data container for specific card creation types.
/// </summary>
public abstract class CardCreationInfo
{
    public required CardStatus CardStatus { get; init; }
    public required string Name { get; init; }
    public required string Id { get; init; }
    public required string Url { get; init; }
    public required DateTimeOffset? CardStatusLastUpdated { get; init; }
    public IEnumerable<ChecklistCreationInfo> ChecklistInfos { get; init; } = [];
    public IEnumerable<CommentInfo> CommentInfos { get; init; } = [];
    public IEnumerable<AttachmentInfo> AttachmentInfos { get; init; } = [];
}