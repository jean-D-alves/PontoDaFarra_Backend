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
    public DbSet<Location> Locations { get; set; } = null!;
    public DbSet<LegalDocument> LegalDocuments { get; set; } = null!;
    public DbSet<UserConsent> UserConsents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasPostgresExtension("pg_trgm");

        modelBuilder.Entity<Product>()
            .HasIndex(p => p.Name)
            .HasMethod("gin")
            .HasOperators("gin_trgm_ops");
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
        modelBuilder.Entity<Location>(e =>
        {
            e.HasKey(x => x.LocationId);
            e.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });
        modelBuilder.Entity<LegalDocument>(e =>
        {
            e.HasKey(x => x.LegalDocumentId);
            e.Property(x => x.Version).HasMaxLength(20).IsRequired();
            e.Property(x => x.Content).IsRequired();
            e.HasIndex(x => new { x.Type, x.Version }).IsUnique();
            e.HasIndex(x => x.Type).IsUnique().HasFilter("\"IsActive\" = true");
        });
        modelBuilder.Entity<UserConsent>(e =>
        {
            e.HasKey(x => x.UserConsentId);
            e.Property(x => x.IpAddress).HasMaxLength(45);
            e.HasIndex(x => new { x.UserId, x.LegalDocumentId }).IsUnique();

            e.HasOne(x => x.User)
             .WithMany(u => u.Consents)
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.LegalDocument)
             .WithMany(d => d.Consents)
             .HasForeignKey(x => x.LegalDocumentId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
