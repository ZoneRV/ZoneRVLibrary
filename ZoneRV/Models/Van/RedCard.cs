namespace ZoneRV.Models.Van;

[DebuggerDisplay("{Name} - {Van.Name}")]
public class RedCard : IFilterableCard
{
    public required string Id { get; init; }

    [JsonIgnore] public required string BoardId { get; init; }
    public required VanProductionInfo Van { get; init; }
    public required string Name { get; init; }
    public required string Url { get; init; }
    public required CardStatus CardStatus { get; set; }
    public float CompletionRate => CardStatus is CardStatus.Completed ? 1f : 0f;

    [JsonIgnore] public DateTimeOffset? CardStatusLastUpdated { get; set; }
    public required RedFlagIssue RedFlagIssue { get; set; }
    public required AreaOfOrigin AreaOfOrigin { get; set; }
    public required DateTimeOffset? CreationDate { get; init; }

    public List<User> Users { get; } = [];
    public List<Comment> Comments { get; } = [];
    public List<Attachment> Attachments { get; } = [];

    public TimeSpan Age => CreationDate.HasValue ? DateTimeOffset.Now - CreationDate.Value : TimeSpan.Zero;
}