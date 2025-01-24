using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace ZoneRV.Services.Trello.Models;

[Index(nameof(ActionId), IsUnique = true)]
public class CachedTrelloAction
{
    [Key, Required]           public          int            Id         { get; set; } 
    [Required, MaxLength(24)] public required string         ActionId   { get; set; }
    [Required, MaxLength(24)] public required string         BoardId    { get; set; }
    [Required, MaxLength(24)] public required string         CardId     { get; set; }
    [Required, MaxLength(24)] public required DateTimeOffset DateOffset { get; set; }
    [Required, MaxLength(48)] public required string         ActionType { get; set; }
    [Required, MaxLength(24)] public required string         MemberId   { get; set; }
    public string? Content { get; set; }
    [MaxLength(24)] public string? CheckId { get; set; }
    public required DateTimeOffset? DueDate { get; set; }
}