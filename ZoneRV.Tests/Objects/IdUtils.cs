namespace ZoneRV.Tests.Objects;

public static class IdUtils
{
    private static int _prouctionLineId = 0;
    
    public static int ProductionLineId
    {
        get
        {
            _prouctionLineId++;
            return _prouctionLineId;
        }
    }
    
    private static int _modelId = 0;
    
    public static int ModelId
    {
        get
        {
            _modelId++;
            return _modelId;
        }
    }
    
    private static int _locationId = 0;
    
    public static int LocationId
    {
        get
        {
            _locationId++;
            return _locationId;
        }
    }
}