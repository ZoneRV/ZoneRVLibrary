using ZoneRV.Serialization;

namespace ZoneRV.Models;

[DebuggerDisplay("{Id}:{Url}")]
public class Attachment
{
    public required string Id { get; init; }
    [JsonIgnore] public required string Url { get; set; }
    //TODO: implement downloading and encrypting the content
    public string Content => "Base 64 string here";
    [OptionalJsonField(true)] public required Card Card { get; init; }   
}

[DebuggerDisplay("{Id} - {Url}")]
public class AttachmentCreationInfo
{
    public required string Id { get; init; }
    public required string Url { get; init; }
}