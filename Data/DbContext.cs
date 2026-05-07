using BarcodeApi.Entities;
using Microsoft.EntityFrameworkCore;

namespace BarcodeApi.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }
public DbSet<User> Users { get; set; }

public DbSet<Company> Companies { get; set; }
public DbSet<UserImage> UserImages { get; set; }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>()
        .HasOne(x => x.Company)
        .WithMany(x => x.Users)
        .HasForeignKey(x => x.CompanyId);
}
}