namespace ZoneRV.Models.Van;

[DebuggerDisplay("{Name} - {Van.Name}")]
public class JobCard : IFilterableCard
{
    public required string Name { get; init; }
    
    public required string Id { get; init; }
    
    [JsonIgnore] public required string BoardId { get; init; }
    public required VanProductionInfo Van { get; init; }
    
    public required string Url { get; init; }

    [JsonIgnore] private TimeSpan _taskTime { get; set; }
    
    public required AreaOfOrigin AreaOfOrigin { get; set; }
    
    [JsonIgnore] private CardStatus _cardStatus;
    
    public DueStatus DueStatus { get; }

    [JsonIgnore]
    public DateTimeOffset? CardStatusLastUpdated
    {
        get;
        private set;
    } // TODO: account for status not being changed and all checks being marked off

    public required ProductionLocation Location { get; set; }
    public List<Checklist> Checklists { get; } = [];

    /// <summary>
    /// Default value of 10 minutes
    /// </summary>
    public TimeSpan TaskTime
    {
        get => _taskTime > TimeSpan.Zero ? _taskTime : TimeSpan.FromMinutes(10);
        
        set => _taskTime = value;
    } 
    
    [JsonIgnore] public TimeSpan RemainingTaskTime => TaskTime - TaskTime * CompletionRate;
    
    public List<Comment> Comments { get; } = [];
    
    public List<Attachment> Attachments { get; } = [];

    [JsonIgnore] public int TotalChecks => AllChecks.Count();
    [JsonIgnore] public int CompletedCheckCount => AllChecks.Count(x => x.IsChecked);
    [JsonIgnore] public int UncompletedCheckCount => AllChecks.Count(x => !x.IsChecked);
    
    [JsonIgnore] public IEnumerable<Check> AllChecks => Checklists.SelectMany(x => x.Checks);
    [JsonIgnore] public float CompletionRate => GetCompletionRate();

    public CardStatus CardStatus
    {
        get
        {
            if (CompletionRate > .999f)
                return CardStatus.Completed;

            if (_cardStatus is CardStatus.UnableToComplete)
                return _cardStatus;

            if (_cardStatus is CardStatus.InProgress || CompletionRate is > 0f and < 1f)
                return CardStatus.InProgress;

            return _cardStatus;
        }
    }
    
    private float GetCompletionRate()
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