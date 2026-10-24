using Microsoft.EntityFrameworkCore;
using PontoDaFarra.models;

namespace PontoDaFarra.context;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(x => x.Email).IsUnique();
        });
        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasIndex(x => x.TokenHash).IsUnique();
            e.Property(x => x.TokenHash).HasMaxLength(64);

            e.HasOne(x => x.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<Order>(e =>
        {
            e.HasKey(x => x.OrderId);
            e.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.SetNull);
            e.HasMany(x => x.Items)
             .WithOne(x => x.Order)
             .HasForeignKey(x => x.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<OrderItem>(e =>
        {
            e.HasKey(x => x.OrderItemId);
            e.HasOne(x => x.Product)
             .WithMany()
             .HasForeignKey(x => x.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
