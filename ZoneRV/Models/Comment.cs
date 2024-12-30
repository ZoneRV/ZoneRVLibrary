namespace ZoneRV.Models;

[DebuggerDisplay("{Content} - {Author.Username}")]
public class Comment
{
    public required string Id { get; init; }

    public string CardId => Card.Id;
    
    [ZoneRVJsonIgnore(JsonIgnoreType.Cache)] public required Card Card { get; init; }

    public string BoardId => Van.Id;
    
    [ZoneRVJsonIgnore(JsonIgnoreType.Cache)] public required VanProductionInfo Van { get; init; }
    
    public required string AuthorId { get; init; }
    
    [ZoneRVJsonIgnore(JsonIgnoreType.Cache)] public User? Author { get; internal set; }
    
    public required DateTimeOffset DateCreated { get; init; }
    
    public required string Content { get; set; }
}

public class CommentInfo
{
    public required string Id { get; set; }
    public required string Content { get; set; }
    public required string AuthorId { get; set; }
    public required DateTimeOffset DateCreated { get; set; }
}