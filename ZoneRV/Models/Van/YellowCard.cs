namespace ZoneRV.Models.Van;

[DebuggerDisplay("{Name} - {Parent.Info.Name}")]
public class YellowCard : Card
{
    public YellowCard(VanProductionInfo van, YellowCardInfo info, AreaOfOrigin areaOfOrigin) : base(van, info, areaOfOrigin)
    {
        CreationDate = info.CreationDate;
    }
    
    public DateTimeOffset? CreationDate { get; init; }
    [ZoneRVJsonIgnore(JsonIgnoreType.Both)] public TimeSpan Age => (CreationDate.HasValue) ? DateTimeOffset.Now - CreationDate.Value : TimeSpan.Zero;
}

public class YellowCardInfo : CardInfo
{
    public required DateTimeOffset? CreationDate { get; set; }
}