namespace ZoneRV.DataAccess.Models;

public class VanID
{
    public required string VanId { get; set; }
    public required string VanName { get; init; }
    public bool Blocked { get; set; } = false;
    public required string Url { get; set; }
}