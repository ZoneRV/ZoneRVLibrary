using ZoneRV.Serialization;

namespace ZoneRV.Models.Production;

/// <summary>
/// Represents a job card in the production system, inheriting from the <see cref="Card"/> base class.
/// A job card encapsulates the details and state necessary to track a specific job within the production workflow.
/// </summary>
/// <remarks>
/// The <see cref="JobCard"/> class includes properties and methods to manage job-related states such as location, due status,
/// task time, and remaining task time. It extends the basic functionality of <see cref="Card"/> by introducing logic
/// specific to production workflows, particularly with regard to task tracking and scheduling.
/// </remarks>
[DebuggerDisplay("{Name} - {SalesOrder.Name}")]
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


    private TimeSpan _taskTime { get; set; }
    
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
/// Represents detailed information required to create a Job Card in the production system.
/// This class is a specialized implementation of the <see cref="CardCreationInfo"/> base class,
/// providing additional properties specific to job-related tasks.
/// </summary>
/// <remarks>
/// A <see cref="JobCardCreationInfo"/> includes all the foundational card creation details,
/// such as status, identification, name, URL, as well as a mandatory property for specifying
/// the time required to complete the associated task.
/// </remarks>
[DebuggerDisplay("{Name}")]
public class JobCardCreationInfo : CardCreationInfo
{
    public required TimeSpan TaskTime { get; init; }
}