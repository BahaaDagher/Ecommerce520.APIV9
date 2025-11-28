using Ecommerce520.APIV9.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce520.APIV9.DataAccess
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<ProductSubImage> ProductSubImages { get; set; }
        public DbSet<ProductColor> ProductColors { get; set; }
        public DbSet<ApplicationUserOTP> ApplicationUserOTPs { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<Promotion> Promotions { get; set; }

        //override protected void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("Data Source=.;initial catalog = Ecommerce520.APIV9520.APIV9520 ;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");
        //}
        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ProductSubImage>().HasKey(p=>new { p.ProductId , p.Img}); 
            modelBuilder.Entity<ProductColor>().HasKey(p=>new { p.ProductId , p.Color}); 
            modelBuilder.Entity<Cart>().HasKey(p=>new { p.ProductId , p.ApplicationUserId}); 
            modelBuilder.Entity<Brand>().Property(b=>b.Img).HasDefaultValue("defaultImg.png"); 
        }
    }
}
