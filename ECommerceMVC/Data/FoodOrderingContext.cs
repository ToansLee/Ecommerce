using Microsoft.EntityFrameworkCore;
using ECommerceMVC.Models;

namespace ECommerceMVC.Data
{
    public class FoodOrderingContext : DbContext
    {
        public FoodOrderingContext(DbContextOptions<FoodOrderingContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<MenuCategory> MenuCategories { get; set; }
        public DbSet<MenuItem> MenuItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<WalletTransaction> WalletTransactions { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Customer
            modelBuilder.Entity<Customer>(eb =>
            {
                eb.HasKey(e => e.Id);
                eb.Property(e => e.Email).HasMaxLength(100).IsRequired();
                eb.Property(e => e.FullName).HasMaxLength(100).IsRequired();
                eb.Property(e => e.Role).HasMaxLength(20).IsRequired();
            });

            // MenuCategory
            modelBuilder.Entity<MenuCategory>(eb =>
            {
                eb.HasKey(e => e.Id);
                eb.Property(e => e.Name).HasMaxLength(100).IsRequired();
            });

            // MenuItem
            modelBuilder.Entity<MenuItem>(eb =>
            {
                eb.HasKey(e => e.Id);
                eb.Property(e => e.Name).HasMaxLength(150).IsRequired();
                eb.HasOne(mi => mi.Category).WithMany(c => c.MenuItems).HasForeignKey(mi => mi.CategoryId).OnDelete(DeleteBehavior.SetNull);
            });

            // Order
            modelBuilder.Entity<Order>(eb =>
            {
                eb.HasKey(e => e.Id);
                eb.HasOne(o => o.Customer).WithMany(c => c.Orders).HasForeignKey(o => o.CustomerId).OnDelete(DeleteBehavior.Restrict);
                eb.HasOne(o => o.Payment).WithOne(p => p.Order).HasForeignKey<Payment>(p => p.OrderId).OnDelete(DeleteBehavior.Cascade);
            });

            // OrderItem
            modelBuilder.Entity<OrderItem>(eb =>
            {
                eb.HasKey(e => e.Id);
                eb.HasOne(oi => oi.Order).WithMany(o => o.Items).HasForeignKey(oi => oi.OrderId).OnDelete(DeleteBehavior.Cascade);
                eb.HasOne(oi => oi.MenuItem).WithMany(mi => mi.OrderItems).HasForeignKey(oi => oi.MenuItemId).OnDelete(DeleteBehavior.Restrict);
            });

            // ChatMessage
            modelBuilder.Entity<ChatMessage>(eb =>
            {
                eb.HasKey(e => e.Id);
                eb.HasOne(c => c.Sender).WithMany().HasForeignKey(c => c.SenderId).OnDelete(DeleteBehavior.Restrict);
                eb.HasOne(c => c.Receiver).WithMany().HasForeignKey(c => c.ReceiverId).OnDelete(DeleteBehavior.Restrict);
            });

            // WalletTransaction
            modelBuilder.Entity<WalletTransaction>(eb =>
            {
                eb.HasKey(e => e.Id);
                eb.HasOne(w => w.Customer).WithMany(c => c.WalletTransactions).HasForeignKey(w => w.CustomerId).OnDelete(DeleteBehavior.Restrict);
                eb.HasOne(w => w.Order).WithMany().HasForeignKey(w => w.OrderId).OnDelete(DeleteBehavior.SetNull);
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}

