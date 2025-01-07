namespace ZoneRV.Models.DB;

public class VanID
{
    public VanID(string name)
    {
        VanName = name;
    }

    public VanID(string name, bool blocked = false, string? id = null, string? url = null)
    {
        VanName = name;
        VanId = id;
        Url = url;
        Blocked = blocked;
    }
    
    public string? VanId { get; set; }
    public string VanName { get; init; }
    public bool Blocked { get; set; } = false;
    public string? Url { get; set; }
}