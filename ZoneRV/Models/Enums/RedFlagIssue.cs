namespace ZoneRV.Models.Enums;

/// <summary>
/// Represents various categories of issues or concerns flagged as "red flags"
/// in a workflow or process on red cards.
/// </summary>
public enum RedFlagIssue
{
    WorkmanShip,
    NonCompletedTask,
    Damage,
    OutOfStock,
    FaultyComponent,
    BuildProcess,
    DesignIssue,
    MissingPart,
    Shortage,
    BOM,
    Unspecified
}