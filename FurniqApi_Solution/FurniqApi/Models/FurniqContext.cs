using Microsoft.EntityFrameworkCore;

namespace FurniqApi.Models;

public class FurniqContext(DbContextOptions<FurniqContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Review> Reviews => Set<Review>();

    public DbSet<Bathroom> Bathroom => Set<Bathroom>();
    public DbSet<Bedroom> Bedroom => Set<Bedroom>();
    public DbSet<Hallway> Hallway => Set<Hallway>();
    public DbSet<Kitchen> Kitchen => Set<Kitchen>();
    public DbSet<Livingroom> Livingroom => Set<Livingroom>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // Az adatbázisban a külső kulcsok ON DELETE CASCADE-ek
        b.Entity<Address>().HasOne(a => a.User).WithMany(u => u.Addresses).HasForeignKey(a => a.UserId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Cart>().HasOne(c => c.User).WithMany(u => u.Carts).HasForeignKey(c => c.UserId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Cart>().HasOne(c => c.Product).WithMany(p => p.Carts).HasForeignKey(c => c.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Order>().HasOne(o => o.User).WithMany(u => u.Orders).HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Order>().HasOne(o => o.Product).WithMany(p => p.Orders).HasForeignKey(o => o.ProductId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Review>().HasOne(r => r.User).WithMany(u => u.Reviews).HasForeignKey(r => r.UserId).OnDelete(DeleteBehavior.Cascade);
        b.Entity<Review>().HasOne(r => r.Product).WithMany(p => p.Reviews).HasForeignKey(r => r.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
