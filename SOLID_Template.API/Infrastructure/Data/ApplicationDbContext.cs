using Microsoft.EntityFrameworkCore;
using SOLID_Template.Domain.Entities;

namespace SOLID_Template.Infrastructure.Data;

/// <summary>
/// Contexto do Entity Framework Core
/// Configura o mapeamento das entidades para o banco de dados
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureProduct(modelBuilder);
        ConfigureOrder(modelBuilder);
        ConfigureOrderProduct(modelBuilder);
    }

    private static void ConfigureProduct(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
                
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
                
            entity.Property(e => e.Sku)
                .IsRequired()
                .HasMaxLength(50);
                
            entity.HasIndex(e => e.Sku)
                .IsUnique();
                
            entity.Property(e => e.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
                
            entity.Property(e => e.StockQuantity)
                .IsRequired();
                
            entity.Property(e => e.Category)
                .HasMaxLength(100);
                
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true);
                
            entity.Property(e => e.CreatedAt)
                .IsRequired();
                
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);
        });
    }

    private static void ConfigureOrder(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Number)
                .IsRequired()
                .HasMaxLength(20);
                
            entity.HasIndex(e => e.Number)
                .IsUnique();
                
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500);
                
            entity.Property(e => e.Amount)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
                
            entity.Property(e => e.OrderDate)
                .IsRequired();
                
            entity.Property(e => e.Status)
                .IsRequired()
                .HasConversion<int>();
                
            entity.Property(e => e.CreatedAt)
                .IsRequired();
                
            entity.Property(e => e.IsDeleted)
                .HasDefaultValue(false);
        });
    }

    private static void ConfigureOrderProduct(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<OrderProduct>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.ProductId });
            
            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderProducts)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.Product)
                .WithMany(p => p.OrderProducts)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.Property(e => e.Quantity)
                .IsRequired()
                .HasDefaultValue(1);
                
            entity.Property(e => e.UnitPrice)
                .IsRequired()
                .HasColumnType("decimal(18,2)");
                
            entity.Property(e => e.Discount)
                .HasColumnType("decimal(18,2)")
                .HasDefaultValue(0);
                
            entity.Property(e => e.AddedDate)
                .IsRequired();
        });
    }
}