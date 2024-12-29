namespace ZoneRV.Models;

[DebuggerDisplay("{Content} - {Author.Username}")]
public class Comment
{
    public required string Id { get; init; }
    
    public required string CardId { get; init; }
    
    [JsonIgnore] public required IFilterableCard Card { get; init; }
    
    public required string BoardId { get; init; }
    
    [JsonIgnore] public required VanBoard Van { get; init; }
    
    [JsonIgnore] public required string AuthorId { get; init; }
    
    public User? Author { get; internal set; }
    
    public required DateTimeOffset DateCreated { get; init; }
    
    public required string Content { get; set; }
}