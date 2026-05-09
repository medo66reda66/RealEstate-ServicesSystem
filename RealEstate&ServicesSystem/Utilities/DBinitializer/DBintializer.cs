using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RealEstate_ServicesSystem.DATABS;
using RealEstate_ServicesSystem.Models;
using RealEstate_ServicesSystem.Utilities.IDBinitializer;


namespace RealEstate_ServicesSystem.Utilities.DBinitializer
{
    public class DBintializer : IDBintializer
    {
        private readonly ApplicationDBcontext _db;
        private readonly UserManager<Applicationuser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<DBintializer> _logger;
        public DBintializer(ApplicationDBcontext db, UserManager<Applicationuser> userManager, RoleManager<IdentityRole> roleManager, ILogger<DBintializer> logger)
        {
            _db = db;
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
        }

        public void Initialize()
        {
            try
            {
                if (_db.Database.GetPendingMigrations().Any())
                {
                    _db.Database.Migrate();
                }
                if (_roleManager.Roles.IsNullOrEmpty())
                {
                    _roleManager.CreateAsync(new(DS.Role_Admin)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(DS.Role_Employee)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(DS.Role_Owner)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(DS.Role_Agent)).GetAwaiter().GetResult();
                    _roleManager.CreateAsync(new(DS.Role_User)).GetAwaiter().GetResult();

                    _userManager.CreateAsync(new Applicationuser
                    {
                        UserName = "Admin",
                        Email = "Admin123@7oDa.com",
                        FullName = "Admin",
                        EmailConfirmed = true,
                        Address = "Admin Address",
                        PhoneNumber = "1234567890"
                    }, "Admin123@").GetAwaiter().GetResult();

                    var USER = _userManager.FindByEmailAsync("Admin123@7oDa.com").GetAwaiter().GetResult();
                    _userManager.AddToRoleAsync(USER!, DS.Role_Admin).GetAwaiter().GetResult();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, "An error occurred while initializing the database.");
            }
        }
    }
}
