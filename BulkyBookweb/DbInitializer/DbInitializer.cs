using BulkyBookweb.Repository.IRepository;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using BulkyBookweb.Data;
using Microsoft.EntityFrameworkCore;
using BulkyBookweb.Models.utility;
using BulkyBookweb.Models;

namespace BulkyBookweb.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDBContext _db;

        public DbInitializer(
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager, AppDBContext appDBContext)
        {
            _userManager = userManager;


            _roleManager = roleManager;
            _db = appDBContext;

        }

        public void Initialize()
        {
            //migration if they are not applid
            try
            {
                if (_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch (Exception ex)
            {

            }
            //create role if they are not created
            if (!_roleManager.RoleExistsAsync(SD.Role_Admin).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Admin)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_Employee)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Indi)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.Role_User_Comp)).GetAwaiter().GetResult();

                //if role not created we will create admin user
                _userManager.CreateAsync(new ApplicationUser
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    Name = "Osaid",
                    PhoneNumber = "0795291818",
                    StreetAddress = "JaferAlhussine",
                    State="Jor",
                    PostalCode="11181",
                    City="jordan"
                },"Osaid13#").GetAwaiter().GetResult();

                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u=>u.Email=="admin@gmail.com");
                _userManager.AddToRoleAsync(user,SD.Role_Admin).GetAwaiter().GetResult();

            }

            return;
        }
    }
}
