namespace ZoneRV.Models.Production;

/// <summary>
/// Represents a YellowCard that inherits from the Card class, providing specific functionality and behavioral traits associated with the YellowCard category.
/// This includes classification, creation details, and age computation for the YellowCard entity.
/// </summary>
[DebuggerDisplay("{SalesOrder.Name}:{Name}")]
public class YellowCard : Card
{
    public override CardType Type
    {
        get => CardType.YellowCard;
    }
    public YellowCard(SalesOrder van, YellowCardCreationInfo info, AreaOfOrigin? areaOfOrigin) : base(van, info, areaOfOrigin)
    {
        CreationDate = info.CreationDate;
    }
    
    public DateTimeOffset? CreationDate { get; init; }
    [JsonIgnore] public TimeSpan Age => (CreationDate.HasValue) ? DateTimeOffset.Now - CreationDate.Value : TimeSpan.Zero;
    
    // TODO: add part information
}

/// <summary>
/// Represents the information required to create or describe a YellowCard.
/// Inherits from the base class CardCreationInfo and includes additional details such as the creation date.
/// This class serves as a blueprint for defining key attributes associated with a YellowCard.
/// </summary>
[DebuggerDisplay("{Name}")]
public class YellowCardCreationInfo : CardCreationInfo
{
    public required DateTimeOffset? CreationDate { get; set; }
}