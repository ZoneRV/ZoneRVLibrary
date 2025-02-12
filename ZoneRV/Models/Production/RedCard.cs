namespace ZoneRV.Models.Production;

/// <summary>
/// Represents a RedCard, a specific type of card used in the production system.
/// It inherits from the base class <see cref="Card"/> and includes properties for
/// <see cref="RedFlagIssue"/> and creation-related details.
/// </summary>
[DebuggerDisplay("{SalesOrder.Name}:{Name}")]
public class RedCard : Card
{
    public override CardType Type
    {
        get => CardType.RedCard;
    }
    public RedCard(SalesOrder van, RedCardCreationInfo info, AreaOfOrigin? areaOfOrigin) : base(van, info, areaOfOrigin)
    {
        RedFlagIssue = info.RedFlagIssue;
        CreationDate = info.CreationDate;
    }
    
    public RedFlagIssue RedFlagIssue { get; set; }
    public DateTimeOffset? CreationDate { get; init; }

    [JsonIgnore] public TimeSpan Age => CreationDate.HasValue ? DateTimeOffset.Now - CreationDate.Value : TimeSpan.Zero;
}

/// <summary>
/// Contains information necessary for the creation of a <see cref="RedCard"/>.
/// Includes specific properties such as <see cref="RedFlagIssue"/> and optional <see cref="CreationDate"/>
/// to represent the associated issue and the card's creation timestamp.
/// Inherits common creation properties from <see cref="CardCreationInfo"/>.
/// </summary>
[DebuggerDisplay("{Name}")]
public class RedCardCreationInfo : CardCreationInfo
{
    public required RedFlagIssue RedFlagIssue { get; set; }
    public required DateTimeOffset? CreationDate { get; set; }
}