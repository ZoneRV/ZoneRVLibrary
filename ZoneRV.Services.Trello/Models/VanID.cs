namespace ZoneRV.Services.Trello.Models;

public class VanId
{
    public VanId(string name)
    {
        VanName = name;
    }

    public VanId(string name, bool blocked = false, string? id = null, string? url = null)
    {
        VanName = name;
        Id = id;
        Url = url;
        Blocked = blocked;
    }
    
    public string? Id { get; set; }
    public string VanName { get; init; }
    public bool Blocked { get; set; } = false;
    public string? Url { get; set; }
}