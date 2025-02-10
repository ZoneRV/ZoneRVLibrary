namespace ZoneRV.Services.Test.Models
{
    public class AreaOfOriginSettings
    {
        public AreaOfOriginSettings(string? name = null, string? description = null, string? icon = null)
        {
            Name       = name;
            Descripion = description;
            Icon       = icon;
        }

        public string? Name       { get; set; }
        public string? Descripion { get; set; }
        public string? Icon       { get; set; }
    }
}