namespace ZoneRV.Models;

[DebuggerDisplay("{FileName}:{Id} - {Url}")]
public class Attachment
{
    public required string Id { get; init; }
    public required string Url { get; set; }
    public required string FileName { get; set; }

    public string CardId => Card.Id;
    public required Card Card { get; init; }
}


public class AttachmentInfo
{
    public required string Id { get; init; }
    public required string Url { get; init; }
    public required string FileName { get; init; }
}