namespace ZoneRV.Models.Production;

[DebuggerDisplay("{Name} Completed:{IsChecked}")]
public class Check
{
    public required string Id { get; init; }
    
    [ZoneRVJsonIgnore(JsonIgnoreType.Cache)] public required Checklist Checklist { get; init; }
    
    public required string Name { get; set; }

    private bool _isChecked;
    public required bool IsChecked 
    { 
        get => _isChecked;
        init => _isChecked = value;
    }
    
    private DateTimeOffset? _lastModified { get; set; }
    public DateTimeOffset? LastModified 
    { 
        get => _lastModified;
        init => _lastModified = value;
    }

    public void UpdateStatus(bool isChecked, DateTimeOffset timeUpdated)
    {
        _isChecked = isChecked;
        _lastModified = timeUpdated;
    }
}

[DebuggerDisplay("{Name} Completed:{IsChecked}")]
public class CheckInfo
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required bool IsChecked { get; set; }
    public required DateTimeOffset? LasUpdated { get; set; }
}