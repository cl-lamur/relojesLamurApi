using Microsoft.EntityFrameworkCore;
using RelojesLamur.API.Entities;

namespace RelojesLamur.API.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductFeatures> ProductFeatures => Set<ProductFeatures>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        // ?? Product ?????????????????????????????????????????
        mb.Entity<Product>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).HasMaxLength(150).IsRequired();
            e.Property(p => p.Description).HasMaxLength(1000).IsRequired();
            e.Property(p => p.Price).HasColumnType("decimal(10,2)").IsRequired();
            e.Property(p => p.Category).HasMaxLength(10).IsRequired();
            e.Property(p => p.ImageUrl).HasMaxLength(500).IsRequired();
            e.Property(p => p.InStock).HasDefaultValue(true);
            e.HasQueryFilter(p => !p.IsDeleted);
        });

        // ?? ProductFeatures ?????????????????????????????????
        mb.Entity<ProductFeatures>(e =>
        {
            e.HasKey(f => f.Id);
            e.Property(f => f.Material).HasMaxLength(200);
            e.Property(f => f.Mechanism).HasMaxLength(200);
            e.Property(f => f.WaterResistance).HasMaxLength(100);
            e.HasOne(f => f.Product)
             .WithOne(p => p.Features)
             .HasForeignKey<ProductFeatures>(f => f.ProductId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // ?? User ????????????????????????????????????????????
        mb.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Name).HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            e.Property(u => u.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(u => u.Role).HasMaxLength(20).IsRequired();
            e.HasIndex(u => u.Email).IsUnique();
            e.HasQueryFilter(u => !u.IsDeleted);
        });

        // ?? Order ???????????????????????????????????????????
        mb.Entity<Order>(e =>
        {
            e.HasKey(o => o.Id);
            e.Property(o => o.Subtotal).HasColumnType("decimal(10,2)");
            e.Property(o => o.Tax).HasColumnType("decimal(10,2)");
            e.Property(o => o.Total).HasColumnType("decimal(10,2)");
            e.Property(o => o.Status).HasMaxLength(20);
            e.HasOne(o => o.User)
             .WithMany(u => u.Orders)
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // ?? OrderItem ???????????????????????????????????????
        mb.Entity<OrderItem>(e =>
        {
            e.HasKey(i => i.Id);
            e.Property(i => i.UnitPrice).HasColumnType("decimal(10,2)");
            e.HasOne(i => i.Order)
             .WithMany(o => o.Items)
             .HasForeignKey(i => i.OrderId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasOne(i => i.Product)
             .WithMany(p => p.OrderItems)
             .HasForeignKey(i => i.ProductId)
             .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
