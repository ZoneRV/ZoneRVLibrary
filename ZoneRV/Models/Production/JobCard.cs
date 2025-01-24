namespace ZoneRV.Models.Production;

[DebuggerDisplay("{Name} - {ProductionInfo.Name}")]
public class JobCard : Card
{
    public JobCard(SalesProductionInfo van, JobCardInfo info, AreaOfOrigin areaOfOrigin, Location.Location location) : base(van, info, areaOfOrigin)
    {
        Location = location;
        _taskTime = info.TaskTime;
    }
    
    public Location.Location Location { get; set; }

    public DueStatus DueStatus
    {
        get
        {
            if (this.Location < this.ProductionInfo.LocationInfo.CurrentLocation)
                return DueStatus.NotDue;
                
            if (this.Location > this.ProductionInfo.LocationInfo.CurrentLocation)
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

[DebuggerDisplay("{Name}")]
public class JobCardInfo : CardInfo
{
    public required TimeSpan TaskTime { get; init; }
}