using Microsoft.EntityFrameworkCore;
using CozyComfort.API.Models.Domain;

namespace CozyComfort.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Blanket> Blankets { get; set; }
        public DbSet<ManufacturerStock> ManufacturerStocks { get; set; }
        public DbSet<Distributor> Distributors { get; set; }
        public DbSet<DistributorStock> DistributorStocks { get; set; }
        public DbSet<Seller> Sellers { get; set; }
        public DbSet<SellerStock> SellerStocks { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<ProductionOrder> ProductionOrders { get; set; }
        public DbSet<SupplyChainQuery> SupplyChainQueries { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.UserId);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Email).HasMaxLength(255).IsRequired();
                entity.Property(u => u.FirstName).HasMaxLength(100);
                entity.Property(u => u.LastName).HasMaxLength(100);
                entity.Property(u => u.Role).HasMaxLength(50);
            });

            modelBuilder.Entity<Blanket>(entity =>
            {
                entity.HasKey(b => b.BlanketId);
                entity.Property(b => b.Name).HasMaxLength(255).IsRequired();
                entity.Property(b => b.Material).HasMaxLength(100);
                entity.Property(b => b.Size).HasMaxLength(50);
                entity.Property(b => b.BasePrice).HasColumnType("decimal(18,2)");
                entity.Property(b => b.ImageUrl).HasMaxLength(500);
            });

            modelBuilder.Entity<ManufacturerStock>(entity =>
            {
                entity.HasKey(ms => ms.ManufacturerStockId);
                entity.HasIndex(ms => ms.BlanketId).IsUnique();
                entity.HasOne(ms => ms.Blanket)
                    .WithOne(b => b.ManufacturerStock)
                    .HasForeignKey<ManufacturerStock>(ms => ms.BlanketId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Distributor>(entity =>
            {
                entity.HasKey(d => d.DistributorId);
                entity.Property(d => d.Name).HasMaxLength(255).IsRequired();
                entity.Property(d => d.ContactEmail).HasMaxLength(255);
                entity.Property(d => d.ContactPhone).HasMaxLength(30);
                entity.Property(d => d.ShippingCost).HasColumnType("decimal(18,2)");
                entity.Property(d => d.SuccessRate).HasColumnType("decimal(5,2)");
                entity.HasOne(d => d.User)
                    .WithOne(u => u.Distributor)
                    .HasForeignKey<Distributor>(d => d.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<DistributorStock>(entity =>
            {
                entity.HasKey(ds => ds.DistributorStockId);
                entity.Property(ds => ds.CostPrice).HasColumnType("decimal(18,2)");
                entity.HasOne(ds => ds.Distributor)
                    .WithMany(d => d.DistributorStocks)
                    .HasForeignKey(ds => ds.DistributorId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ds => ds.Blanket)
                    .WithMany(b => b.DistributorStocks)
                    .HasForeignKey(ds => ds.BlanketId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Seller>(entity =>
            {
                entity.HasKey(s => s.SellerId);
                entity.Property(s => s.Name).HasMaxLength(255).IsRequired();
                entity.Property(s => s.ContactEmail).HasMaxLength(255);
                entity.Property(s => s.ContactPhone).HasMaxLength(30);
                entity.Property(s => s.ShippingAddress).HasMaxLength(500);
                entity.HasOne(s => s.User)
                    .WithOne(u => u.Seller)
                    .HasForeignKey<Seller>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(s => s.AssignedDistributor)
                    .WithMany(d => d.Sellers)
                    .HasForeignKey(s => s.AssignedDistributorId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<SellerStock>(entity =>
            {
                entity.HasKey(ss => ss.SellerStockId);
                entity.Property(ss => ss.RetailPrice).HasColumnType("decimal(18,2)");
                entity.HasOne(ss => ss.Seller)
                    .WithMany(s => s.SellerStocks)
                    .HasForeignKey(ss => ss.SellerId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(ss => ss.Blanket)
                    .WithMany(b => b.SellerStocks)
                    .HasForeignKey(ss => ss.BlanketId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.OrderId);
                entity.Property(o => o.OrderReference).HasMaxLength(50).IsRequired();
                entity.Property(o => o.Status).HasMaxLength(50);
                entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(o => o.DeliveryAddress).HasMaxLength(500);
                entity.Property(o => o.CustomerName).HasMaxLength(200);
                entity.Property(o => o.CustomerEmail).HasMaxLength(255);
                entity.HasOne(o => o.Seller)
                    .WithMany(s => s.Orders)
                    .HasForeignKey(o => o.SellerId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(o => o.Distributor)
                    .WithMany(d => d.Orders)
                    .HasForeignKey(o => o.DistributorId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasMany(o => o.OrderItems)
                    .WithOne(oi => oi.Order)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(o => o.ProductionOrders)
                    .WithOne(po => po.Order)
                    .HasForeignKey(po => po.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.OrderItemId);
                entity.Property(oi => oi.UnitPrice).HasColumnType("decimal(18,2)");
                entity.Property(oi => oi.TotalPrice).HasColumnType("decimal(18,2)");
                entity.HasOne(oi => oi.Blanket)
                    .WithMany()
                    .HasForeignKey(oi => oi.BlanketId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ProductionOrder>(entity =>
            {
                entity.HasKey(po => po.ProductionOrderId);
                entity.Property(po => po.Status).HasMaxLength(50);
                entity.HasOne(po => po.Distributor)
                    .WithMany(d => d.ProductionOrders)
                    .HasForeignKey(po => po.DistributorId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(po => po.Blanket)
                    .WithMany()
                    .HasForeignKey(po => po.BlanketId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<SupplyChainQuery>(entity =>
            {
                entity.HasKey(q => q.SupplyChainQueryId);
                entity.Property(q => q.DistributorCostQuoted).HasColumnType("decimal(18,2)");
                entity.HasOne(q => q.Seller)
                    .WithMany()
                    .HasForeignKey(q => q.SellerId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(q => q.Blanket)
                    .WithMany()
                    .HasForeignKey(q => q.BlanketId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
