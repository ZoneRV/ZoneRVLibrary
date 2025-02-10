namespace ZoneRV.Services.Test.Models;

public class ModelSettings
{
    public ModelSettings(string? name = null, string? description = null, string? prefix = null)
    {
        Name        = name;
        Description = description;
        Prefix      = prefix;
    }
    
    public string? Name        { get; set; }
    public string? Description { get; set; }
    public string? Prefix      { get; set; }
}