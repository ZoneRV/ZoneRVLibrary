namespace ZoneRV.Models.Production;

/// <summary>
/// Represents a RedCard used in the production system.
/// A RedCard is a specific type of <see cref="Card"/> that identifies production-related
/// issues or concerns, such as those defined in <see cref="RedFlagIssue"/>.
/// </summary>
[DebuggerDisplay("{Name} - {SalesOrder.Name}")]
public class RedCard : Card
{
    public RedCard(SalesOrder van, RedCardCreationInfo info, AreaOfOrigin? areaOfOrigin) : base(van, info, areaOfOrigin)
    {
        RedFlagIssue = info.RedFlagIssue;
        CreationDate = info.CreationDate;
    }
    
    public RedFlagIssue? RedFlagIssue { get; set; }
    public DateTimeOffset? CreationDate { get; init; }

    [JsonIgnore] public TimeSpan Age => CreationDate.HasValue ? DateTimeOffset.Now - CreationDate.Value : TimeSpan.Zero;
}

/// <summary>
/// Represents the creation information for a RedCard in the production system.
/// This class extends the base class <see cref="CardCreationInfo"/> and adds
/// additional properties specific to a RedCard.
/// </summary>
[DebuggerDisplay("{Name}")]
public class RedCardCreationInfo : CardCreationInfo
{
    public required RedFlagIssue? RedFlagIssue { get; set; }
    public required DateTimeOffset? CreationDate { get; set; }
}