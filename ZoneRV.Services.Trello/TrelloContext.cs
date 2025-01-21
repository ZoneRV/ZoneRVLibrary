using Microsoft.EntityFrameworkCore;
using ZoneRV.Services.Trello.Models;

namespace ZoneRV.Services.Trello;

public class TrelloContext : DbContext
{
    public required DbSet<CachedTrelloAction> Actions { get; set; }
    public required DbSet<VanId>              VanIds  { get; set; }
    
    
    public TrelloContext(DbContextOptions<TrelloContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Configure default schema
        modelBuilder.HasDefaultSchema("trello");
    }
}