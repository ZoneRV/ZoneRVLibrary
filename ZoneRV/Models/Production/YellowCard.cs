namespace ZoneRV.Models.Production;

/// <summary>
/// Represents a specialized type of card, specifically a YellowCard, within the system.
/// A YellowCard inherits from the base Card class and includes additional properties and behavior specific to this type of object.
/// </summary>
[DebuggerDisplay("{Name} - {Parent.Info.Name}")]
public class YellowCard : Card
{
    public YellowCard(SalesProductionInfo van, YellowCardInfo info, AreaOfOrigin? areaOfOrigin) : base(van, info, areaOfOrigin)
    {
        CreationDate = info.CreationDate;
    }
    
    public DateTimeOffset? CreationDate { get; init; }
    public TimeSpan Age => (CreationDate.HasValue) ? DateTimeOffset.Now - CreationDate.Value : TimeSpan.Zero;
}

/// <summary>
/// Represents detailed information specific to the creation or configuration of a YellowCard within the system.
/// This class includes attributes essential for initializing a YellowCard, such as its creation date and inherited properties from the CardCreationInfo base class.
/// </summary>
[DebuggerDisplay("{Name}")]
public class YellowCardInfo : CardCreationInfo
{
    public required DateTimeOffset? CreationDate { get; set; }
}