using DevTask.DiscountService.Server.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DevTask.DiscountService.Server.Data;

public class DiscountContext : DbContext, IDesignTimeDbContextFactory<DiscountContext>
{
    public DbSet<DiscountCode> DiscountCodes { get; set; }
    public DiscountContext(DbContextOptions<DiscountContext> options)
        : base(options)
    {
    }

    #region Constructors for DB Migrations
    public DiscountContext()
    {        
    }

    /// <summary>
    /// Constructor used to add DB Migration
    /// </summary>
    public DiscountContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<DiscountContext>();
        optionsBuilder.UseSqlServer("Server=127.0.0.1;Database=DiscountDB;User Id=sa;Password=Your_Strong_Password123;TrustServerCertificate=True");
        return new DiscountContext(optionsBuilder.Options);
    }
    #endregion Constructors for DB Migrations

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DiscountCode>().HasIndex(x => x.Code).IsUnique();
        base.OnModelCreating(modelBuilder);
    }
}