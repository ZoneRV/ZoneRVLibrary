using ZoneRV.Serialization;

namespace ZoneRV.Models;

[DebuggerDisplay("{Id}:{Url}")]
public class Attachment
{
    public required              string Id      { get; init; }
    [JsonIgnore] public required string Url     { get; set; }
    [FilterableField] public     string Content => throw new NotImplementedException();
    public required              Card   Card    { get; init; }
}

[DebuggerDisplay("{Id} - {Url}")]
public class AttachmentInfo
{
    public required string Id { get; init; }
    public required string Url { get; init; }
}