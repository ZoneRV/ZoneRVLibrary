namespace ZoneRV.Services.Test.Models;

public class LineSettings
{
    public LineSettings(string? name = null, string? description = null, IEnumerable<AreaOfOriginSettings>? areas = null, IEnumerable<ModelSettings>? models = null)
    {
        Name          = name;
        Description   = description;
        AreaSettings  = areas is null ? [] : areas.ToList();
        ModelSettings = models is null ? [] : models.ToList();
    }
    
    public string? Name        { get; set; }
    public string? Description { get; set; }
    
    public List<AreaOfOriginSettings>? AreaSettings { get; set; }
    public List<ModelSettings>? ModelSettings { get; set; }
}