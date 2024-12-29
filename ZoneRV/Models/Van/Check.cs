namespace ZoneRV.Models.Van;

[DebuggerDisplay("{Name}")]
public class Check
{
    public required string Id { get; init; }
    
    [JsonIgnore] public required string BoardId { get; init; }
    public required VanProductionInfo Van { get; init; } 
    
    [JsonIgnore] public required string CheckListId { get; init; }
    public required Checklist Checklist { get; init; }
    
    public required string Name { get; set; }

    public required bool IsChecked 
    { 
        get => _isChecked;
        init => _isChecked = value;
    }
    
    private bool _isChecked;
    
    public DateTimeOffset? LastModified { get; private set; }

    public void UpdateStatus(bool isChecked, DateTimeOffset timeUpdated)
    {
        _isChecked = isChecked;
        LastModified = timeUpdated;
    }
}