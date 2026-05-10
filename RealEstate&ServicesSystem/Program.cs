using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using RealEstate_ServicesSystem.DATABS;
using RealEstate_ServicesSystem.Models;
using RealEstate_ServicesSystem.NotificationSignalR;
using RealEstate_ServicesSystem.Repository;
using RealEstate_ServicesSystem.Repository.IRepository;
using RealEstate_ServicesSystem.Utilities;
using RealEstate_ServicesSystem.Utilities.DBinitializer;
using RealEstate_ServicesSystem.Utilities.IDBinitializer;
using Stripe;
using System.Security.Permissions;

namespace RealEstate_ServicesSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<ApplicationDBcontext>(options =>
            {
                options.UseSqlServer(builder.Configuration.GetConnectionString("default"));
            });

            builder.Services.AddSignalR();
            builder.Services.AddAuthentication()
                .AddGoogle("google", opt =>
                {
                    var googleAuth = builder.Configuration.GetSection("Authentication:Google");
                    opt.ClientId = googleAuth["ClientId"];
                    opt.ClientSecret = googleAuth["ClientSecret"];
                    opt.SignInScheme = IdentityConstants.ExternalScheme;
                });

            builder.Services.AddIdentity<Applicationuser, IdentityRole>(option =>
            {
                    option.Password.RequireDigit = false;
                    option.Password.RequireLowercase = false;
                    option.Password.RequireUppercase = false;
                    option.Password.RequireNonAlphanumeric = false;
                    option.Password.RequiredLength = 6;
                    option.User.RequireUniqueEmail = true;
                    option.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<ApplicationDBcontext>()
            .AddDefaultTokenProviders();
            builder.Services.AddTransient<IEmailSender,EmailSender>();

            StripeConfiguration.ApiKey = builder.Configuration["Stripe:SecretKey"];

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Identity/Account/Login";
                options.AccessDeniedPath = "/Identity/Account/AccessDenied";
            });

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<IRepository<Property>, Repository<Property>>();
            builder.Services.AddScoped<IRepository<Unit>, Repository<Unit>>();
            builder.Services.AddScoped<IRepository<Listing>, Repository<Listing>>();
            builder.Services.AddScoped<IRepository<UnitSupImg>, Repository<UnitSupImg>>();
            builder.Services.AddScoped<IRepository<Otps>, Repository<Otps>>();
            builder.Services.AddScoped<IRepository<Applicationuser>, Repository<Applicationuser>>();
            builder.Services.AddScoped<IRepository<IdentityRole>, Repository<IdentityRole>>();
            builder.Services.AddScoped<IRepository<Userrequest>, Repository<Userrequest>>();
            builder.Services.AddScoped<IRepository<Notification>, Repository<Notification>>();
            builder.Services.AddScoped<IRepository<UserReview>, Repository<UserReview>>();
            builder.Services.AddScoped<IRepository<Favorite>, Repository<Favorite>>();
            builder.Services.AddScoped<ISupImgRepository, SupImgRepository>();
            builder.Services.AddScoped<IDBintializer, DBintializer>();

            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            var app = builder.Build();
            var SCOPE = app.Services.CreateScope();
            var DBinitializer = SCOPE.ServiceProvider.GetService<IDBintializer>();
            DBinitializer!.Initialize();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
                
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseSession();

            app.UseStaticFiles();

            app.UseAuthorization();
            app.MapHub<NotificationHub>("/notificationHub");

            app.MapStaticAssets();
            app.MapControllerRoute(
                name: "default",
                pattern: "{area=user}/{controller=Home}/{action=Index}/{id?}")
                .WithStaticAssets();

            app.Run();
        }

        private static void AddDefaultTokenProviders()
        {
            throw new NotImplementedException();
        }
    }
}
