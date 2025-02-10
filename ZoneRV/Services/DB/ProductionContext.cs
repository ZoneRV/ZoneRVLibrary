using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace ZoneRV.DBContexts;

public class ProductionContext : DbContext
{
    public virtual DbSet<ProductionWorkspace> Workspaces           { get; set; }
    public virtual DbSet<WorkspaceLocation>   WorkSpaceLocations   { get; set; }
    public virtual DbSet<ProductionLine>      Lines                { get; set; }
    public virtual DbSet<OrderedLineLocation> OrderedLineLocations { get; set; }
    public virtual DbSet<AreaOfOrigin>        AreaOfOrigin         { get; set; }
    public virtual DbSet<Model>               Models               { get; set; }
                                           
                                                           
    public ProductionContext(DbContextOptions<ProductionContext> options) : base(options)
    {
    }

    public ProductionContext()
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

        modelBuilder.Entity<OrderedLineLocation>()
                    .Property(o => o.Order)
                    .HasPrecision(12, 10);
        
        //Configure default schema
        modelBuilder.HasDefaultSchema("production");
        
        base.OnModelCreating(modelBuilder);
    }
}