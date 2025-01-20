using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ZoneRV.DBContexts;

public class ProductionContext : DbContext
{
    public DbSet<AreaOfOrigin> AreaOfOrigin { get; set; }
    public DbSet<ProductionLine> Lines { get; set; }
    public DbSet<ProductionLocation> Locations { get; set; }
    public DbSet<Model> Models { get; set; }
}