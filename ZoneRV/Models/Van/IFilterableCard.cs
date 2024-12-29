namespace ZoneRV.Models.Van;

public interface IFilterableCard
{
    public string Name { get; }
    public string BoardId { get; }
    public AreaOfOrigin AreaOfOrigin { get; }
    public CardStatus CardStatus { get; }
    public float CompletionRate { get; }
}