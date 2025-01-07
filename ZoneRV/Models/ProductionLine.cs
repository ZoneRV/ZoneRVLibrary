namespace ZoneRV.Models.Enums;

public class ProductionLine
{
    public required int Id { get; set; }
    public required string Name { get; set; }

    public List<VanModel> Models { get; set; } = [];

    public static bool operator ==(ProductionLine first, ProductionLine second)
        => first.Id == second.Id;

    public static bool operator !=(ProductionLine first, ProductionLine second)
        => first.Id != second.Id;
}