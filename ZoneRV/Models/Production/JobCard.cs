using ZoneRV.Serialization;

namespace ZoneRV.Models.Production;

/// <summary>
/// Represents a production job card containing task and location-specific details
/// associated with a sales order in the production system.
/// </summary>
/// <remarks>
/// A <see cref="JobCard"/> is derived from the <see cref="Card"/> base class and includes
/// properties such as location, task time, and due status. It determines task progress
/// and calculates any remaining task time. Job cards are also connected to ordered line
/// locations to track their association with production workflows.
/// </remarks>
[DebuggerDisplay("{SalesOrder.Name}:{Name}")]
public class JobCard : Card
{
    public override CardType Type
    {
        get => CardType.JobCard;
    }
    
    public JobCard(SalesOrder van, JobCardCreationInfo info, AreaOfOrigin? areaOfOrigin, OrderedLineLocation lineLocation) : base(van, info, areaOfOrigin)
    {
        Location = lineLocation;
        _taskTime = info.TaskTime;
    }
    
    [OptionalJsonField(true)] public OrderedLineLocation Location { get; set; }

    public DueStatus DueStatus
    {
        get
        {
            if (this.SalesOrder.LocationInfo.CurrentLocation is null)
                return DueStatus.NotDue;
            
            if (this.Location > this.SalesOrder.LocationInfo.CurrentLocation)
                return DueStatus.NotDue;
                
            if (this.Location < this.SalesOrder.LocationInfo.CurrentLocation)
                return DueStatus.OverDue;
            
            return DueStatus.Due;
        }
    }


    private TimeSpan _taskTime;
    
    /// <summary>
    /// Default value of 10 minutes
    /// </summary>
    public TimeSpan TaskTime
    {
        get => _taskTime > TimeSpan.Zero ? _taskTime : TimeSpan.FromMinutes(10);
        
        set => _taskTime = value;
    } 
    
    public TimeSpan RemainingTaskTime => TaskTime - TaskTime * CardProgress;

}

/// <summary>
/// Provides details required for creating a <see cref="JobCard"/>, including
/// essential configuration such as task duration and unique identification.
/// </summary>
/// <remarks>
/// This class extends <see cref="CardCreationInfo"/> and encapsulates additional
/// information specific to job card creation, such as the time allocated for tasks.
/// Used during the generation of job cards in production workflows.
/// </remarks>
[DebuggerDisplay("{Name}")]
public class JobCardCreationInfo : CardCreationInfo
{
    public required TimeSpan TaskTime { get; init; }
}