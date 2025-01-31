using ZoneRV.Serialization;

namespace ZoneRV.Models.Production;

/// <summary>
/// Represents a specific item or task within a checklist that can be tracked for completion status.
/// </summary>
/// <remarks>
/// The Check class encapsulates properties and behavior associated with an individual task,
/// such as its name, completion status, and timestamps. It is designed to work within the context
/// of a checklist and is used to track and update task state dynamically through the production service.
/// </remarks>
/// <seealso cref="Checklist"/>
/// <seealso cref="CheckCreationInfo"/>
[DebuggerDisplay("{Name} Completed:{IsChecked}")]
public class Check
{
    public required string Id { get; init; }
    
    [OptionalJsonField(true)] public required Checklist Checklist { get; init; }
    
    public required string Name { get; set; }

    private bool _isChecked;
    public required bool IsChecked 
    { 
        get => _isChecked;
        init => _isChecked = value;
    }
    
    [JsonIgnore] private DateTimeOffset? _lastModified { get; set; }
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

/// <summary>
/// Represents the information required to create a new check entry.
/// </summary>
/// <remarks>
/// This class is used as a data transfer object to encapsulate the necessary properties
/// for creating a check within a checklist. It is primarily used in conjunction with methods
/// that perform check creation in the production service layer.
/// </remarks>
/// <seealso cref="Check"/>
/// <seealso cref="Checklist"/>
[DebuggerDisplay("{Name} Completed:{IsChecked}")]
public class CheckCreationInfo
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required bool IsChecked { get; set; }
    public required DateTimeOffset? LasUpdated { get; set; }
}