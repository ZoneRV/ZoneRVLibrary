namespace ZoneRV.Models.Production;

[DebuggerDisplay("{Name} - {Parent.Info.Name}")]
public class YellowCard : Card
{
    public YellowCard(SalesProductionInfo van, YellowCardInfo info, AreaOfOrigin areaOfOrigin) : base(van, info, areaOfOrigin)
    {
        CreationDate = info.CreationDate;
    }
    
    public DateTimeOffset? CreationDate { get; init; }
    public TimeSpan Age => (CreationDate.HasValue) ? DateTimeOffset.Now - CreationDate.Value : TimeSpan.Zero;
}

[DebuggerDisplay("{Name}")]
public class YellowCardInfo : CardCreationInfo
{
    public required DateTimeOffset? CreationDate { get; set; }
}