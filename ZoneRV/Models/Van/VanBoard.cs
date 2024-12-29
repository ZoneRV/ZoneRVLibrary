namespace ZoneRV.Models.Van;

[DebuggerDisplay("{Info.Name} - {Info.Id}")]
public class VanBoard
{
    public required VanProductionInfo Info { get; init; }
    
    public List<JobCard> JobCards { get; } = [];
    public List<RedCard> RedCards { get; } = [];
    public List<YellowCard> YellowCards { get; } = [];

    public IEnumerable<IFilterableCard> Cards => JobCards.Select(IFilterableCard (x) => x).Concat(RedCards).Concat(YellowCards);

    [JsonIgnore] public double CompletionRate => Cards.Any() ? Cards.Average(x => x.CompletionRate) : 0;
}