namespace ZoneRV.Models;

[DebuggerDisplay("{Username}")]
public class User
{
    public required string Id { get; init; }
    
    public required string FullName { get; set; }
    
    public required string Username { get; set; }
    
    public required string AvatarUrl { get; set; }
}