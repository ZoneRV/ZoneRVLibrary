namespace ZoneRV.DataAccess.Models;

public class CachedTrelloAction
{
    public required string ActionId { get; set; }
    public required string BoardId { get; set; }
    public required string CardId { get; set; }
    public required DateTimeOffset DateOffset { get; set; }
    public required string ActionType { get; set; }
    public required string MemberId { get; set; }
    public string? Content { get; set; }
    public string? CheckId { get; set; }
    public required DateTimeOffset? DueDate { get; set; }
}