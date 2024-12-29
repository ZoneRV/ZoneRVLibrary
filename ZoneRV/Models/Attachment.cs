namespace ZoneRV.Models;

[DebuggerDisplay("{FileName}:{Id} - {Url}")]
public class Attachment
{
    public required string Id { get; init; }
    public required string Url { get; set; }
    public required string FileName { get; set; }
}