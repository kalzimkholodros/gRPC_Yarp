using Microsoft.EntityFrameworkCore;
using BasketService.Models;

namespace BasketService.Data
{
    public class BasketDbContext : DbContext
    {
        public BasketDbContext(DbContextOptions<BasketDbContext> options) : base(options)
        {
        }

        public DbSet<Basket> Baskets { get; set; }
        public DbSet<BasketItem> BasketItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Basket>()
                .HasMany(b => b.Items)
                .WithOne()
                .HasForeignKey(i => i.BasketId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 