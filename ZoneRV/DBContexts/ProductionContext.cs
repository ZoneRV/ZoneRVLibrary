using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ZoneRV.DBContexts;

public class ProductionContext : DbContext
{
    public DbSet<AreaOfOrigin> AreaOfOrigin { get; set; }
    public DbSet<ProductionLine> Lines { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Model> Models { get; set; }

    public ProductionContext(DbContextOptions<ProductionContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        //Configure default schema
        modelBuilder.HasDefaultSchema("production");
    }
}