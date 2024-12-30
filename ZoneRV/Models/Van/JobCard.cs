namespace ZoneRV.Models.Van;

[DebuggerDisplay("{Name} - {Van.Name}")]
public class JobCard : Card
{
    public JobCard(VanProductionInfo van, JobCardInfo info, AreaOfOrigin areaOfOrigin, ProductionLocation location) : base(van, info, areaOfOrigin)
    {
        Location = location;
        _taskTime = info.TaskTime;
    }
    
    public ProductionLocation Location { get; set; }

    public DueStatus DueStatus
    {
        get
        {
            if (this.Location < this.Van.LocationInfo.CurrentLocation)
                return DueStatus.NotDue;
                
            if (this.Location > this.Van.LocationInfo.CurrentLocation)
                return DueStatus.OverDue;
            
            return DueStatus.Due;
        }
    }


    [ZoneRVJsonIgnore(JsonIgnoreType.Api)] private TimeSpan _taskTime { get; set; }
    
    /// <summary>
    /// Default value of 10 minutes
    /// </summary>
    [ZoneRVJsonIgnore(JsonIgnoreType.Cache)] public TimeSpan TaskTime
    {
        get => _taskTime > TimeSpan.Zero ? _taskTime : TimeSpan.FromMinutes(10);
        
        set => _taskTime = value;
    } 
    
    [ZoneRVJsonIgnore(JsonIgnoreType.Both)] public TimeSpan RemainingTaskTime => TaskTime - TaskTime * GetCompletionRate();

}

public class JobCardInfo : CardInfo
{
    public required TimeSpan TaskTime { get; init; }
}