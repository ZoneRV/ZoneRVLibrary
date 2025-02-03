using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ZoneRV.DBContexts;

public class ProductionContext : DbContext
{
    public DbSet<ProductionWorkspace> Workspaces           { get; set; }
    public DbSet<WorkspaceLocation>   WorkSpaceLocations   { get; set; }
    public DbSet<ProductionLine>      Lines                { get; set; }
    public DbSet<OrderedLineLocation> OrderedLineLocations { get; set; }
    public DbSet<AreaOfOrigin>        AreaOfOrigin         { get; set; }
    public DbSet<Model>               Models               { get; set; }
                                                           
    public ProductionContext(DbContextOptions<ProductionContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ProductionWorkspace>()
                    .HasIndex(x => x.Name)
                    .IsUnique();

        modelBuilder.Entity<ProductionWorkspace>()
                    .HasMany<ProductionLine>(x => x.Lines)
                    .WithOne(x => x.Workspace);
        
        //Configure default schema
        modelBuilder.HasDefaultSchema("production");
    }
}