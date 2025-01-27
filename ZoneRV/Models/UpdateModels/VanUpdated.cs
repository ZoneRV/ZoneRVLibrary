using Azure.Core;

namespace ZoneRV.Models.UpdateModels;

public class VanUpdated : IBaseUpdate
{
    public required string Id { get; set; }
    public required IBaseUpdate UpdateType { get; set; }
}