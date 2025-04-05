using Microsoft.EntityFrameworkCore;
using E_commerce_pubg_api.Domain.Entities;

namespace E_commerce_pubg_api.Infrastructure.Persistence
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Category>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Description).HasMaxLength(500);
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Description).HasMaxLength(500);
                entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Category).HasMaxLength(50);
            });

            modelBuilder.Entity<ProductImage>(entity =>
            {
                entity.Property(e => e.ImageUrl).IsRequired().HasMaxLength(250);
                entity.Property(e => e.PublicId).IsRequired().HasMaxLength(100);
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.Images)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is Product && (
                    e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (Product)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = DateTime.UtcNow;
                }
                entity.UpdatedAt = DateTime.UtcNow;
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}