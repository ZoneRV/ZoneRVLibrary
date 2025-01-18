namespace ZoneRV.Models.Van;

[DebuggerDisplay("{Name} - {Van.Name}")]
public class RedCard : Card
{
    public RedCard(VanProductionInfo van, RedCardInfo info, AreaOfOrigin areaOfOrigin) : base(van, info, areaOfOrigin)
    {
        RedFlagIssue = info.RedFlagIssue;
        CreationDate = info.CreationDate;
    }
    
    public RedFlagIssue? RedFlagIssue { get; set; }
    public DateTimeOffset? CreationDate { get; init; }

    [ZoneRVJsonIgnore(JsonIgnoreType.Both)] public TimeSpan Age => CreationDate.HasValue ? DateTimeOffset.Now - CreationDate.Value : TimeSpan.Zero;
}

[DebuggerDisplay("{Name}")]
public class RedCardInfo : CardInfo
{
    public required RedFlagIssue? RedFlagIssue { get; set; }
    public required DateTimeOffset? CreationDate { get; set; }
}