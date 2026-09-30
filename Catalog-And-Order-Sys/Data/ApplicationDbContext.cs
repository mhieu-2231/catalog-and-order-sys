using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Catalog_And_Order_Sys.Models;

namespace Catalog_And_Order_Sys.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 1. Cấu hình bảng Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("Category");
                entity.HasKey(c => c.CategoryId);

                entity.Property(c => c.CategoryName)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(c => c.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");

                // Chỉ hiển thị danh mục chưa xóa mềm ở mọi truy vấn mặc định
                entity.HasQueryFilter(c => !c.IsDeleted);
            });

            // 2. Cấu hình bảng Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("Product");
                entity.HasKey(p => p.ProductId);

                entity.Property(p => p.ProductName)
                      .IsRequired()
                      .HasMaxLength(200);

                entity.Property(p => p.Price)
                      .HasColumnType("decimal(18,2)");

                entity.Property(p => p.CreatedAt)
                      .HasDefaultValueSql("GETDATE()");

                entity.HasIndex(p => p.ProductName).HasDatabaseName("IX_Product_Name");

                // Foreign Key Category -> Product (1-N), không cho xóa cứng Category
                // khi vẫn còn Product tham chiếu
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Chỉ hiển thị sản phẩm chưa xóa mềm ở mọi truy vấn mặc định
                entity.HasQueryFilter(p => !p.IsDeleted);
            });

            // 3. Cấu hình bảng Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("Order");
                entity.HasKey(o => o.OrderId);

                entity.Property(o => o.CustomerName).IsRequired().HasMaxLength(150);
                entity.Property(o => o.PhoneNumber).IsRequired().HasMaxLength(15);
                entity.Property(o => o.Address).IsRequired().HasMaxLength(255);
                entity.Property(o => o.TotalAmount).HasColumnType("decimal(18,2)");
                entity.Property(o => o.Status).HasMaxLength(30).HasDefaultValue("Chờ xử lý");
                entity.Property(o => o.OrderDate).HasDefaultValueSql("GETDATE()");
            });

            // 4. Cấu hình bảng OrderDetail
            modelBuilder.Entity<OrderDetail>(entity =>
            {
                entity.ToTable("OrderDetail");
                entity.HasKey(od => od.OrderDetailId);

                entity.Property(od => od.UnitPrice).HasColumnType("decimal(18,2)");

                entity.HasOne(od => od.Order)
                      .WithMany(o => o.OrderDetails)
                      .HasForeignKey(od => od.OrderId)
                      .OnDelete(DeleteBehavior.Cascade); // xóa Order thì xóa luôn OrderDetail

                entity.HasOne(od => od.Product)
                      .WithMany(p => p.OrderDetails)
                      .HasForeignKey(od => od.ProductId)
                      .OnDelete(DeleteBehavior.Restrict); // không cho xóa cứng Product đã có đơn hàng
            });
        }
    }
}
