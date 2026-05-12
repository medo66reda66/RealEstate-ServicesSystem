using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RealEstate_ServicesSystem.Models;
using RealEstate_ServicesSystem.Viewmodel;

namespace RealEstate_ServicesSystem.DATABS
{
    public class ApplicationDBcontext : IdentityDbContext<Applicationuser>
    {
        public ApplicationDBcontext(DbContextOptions<ApplicationDBcontext> options) : base(options)
        {
        }
        public DbSet<Property> Properties { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<Listing> Listings { get; set; }
        public DbSet<Userrequest> Userrequests { get; set; }
        public DbSet<UserReview> UserReviews { get; set; }
        public DbSet<UnitSupImg> UnitSupImgs { get; set; }
        public DbSet<Otps> Otps { get; set; } 
        public DbSet<Notification> notifications { get; set; } 
        public DbSet<Favorite> Favorites { get; set; } 
        public DbSet<Massage> Massages { get; set; } 

        override protected void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
       
        
        //override protected void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseSqlServer("Data Source=.;Initial catalog=RealEstateServicesDB;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False");

        //}
    }
}
