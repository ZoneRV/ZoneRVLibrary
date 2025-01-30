namespace ZoneRV.Models;

[DebuggerDisplay("{Content} - {Author.Username}")]
public class Comment
{
    public required string Id { get; init; }
    
    public required Card Card { get; init; }
    
    public User? Author { get; internal set; }
    
    public required DateTimeOffset DateCreated { get; init; }
    
    public required string Content { get; set; }
}

[DebuggerDisplay("{Content} - AuthorId:{AuthorId}")]
public class CommentInfo
{
    public required string Id { get; set; }
    public required string Content { get; set; }
    public required string AuthorId { get; set; }
    public required DateTimeOffset DateCreated { get; set; }
}