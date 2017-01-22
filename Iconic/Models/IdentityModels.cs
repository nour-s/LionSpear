using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Data.Entity.Migrations;

namespace Iconic.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager, string authenticationType)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        static ApplicationDbContext()
        {
            Database.SetInitializer<ApplicationDbContext>(new IdentitySeeder());
        }

        public ApplicationDbContext()
            : base("IconicDbContext", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }
    }

    public class IdentitySeeder : CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            base.Seed(context);

            //The UserStore is ASP Identity's data layer. Wrap context with the UserStore.
            UserStore<ApplicationUser> userStore = new UserStore<ApplicationUser>(context);

            UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(userStore);

            //Add or Update the initial Users into the database as normal.
            context.Users.AddOrUpdate(
                x => x.Email,  //Using Email as the Unique Key: If a record exists with the same email, AddOrUpdate skips it.
                new ApplicationUser() { Email = "n@m.com", UserName = "nour", PasswordHash = new PasswordHasher().HashPassword("!@#123aA") },
                new ApplicationUser() { Email = "s@r.com", UserName = "user1", PasswordHash = new PasswordHasher().HashPassword("!@#123aA") }
            );

            context.SaveChanges();

            //Get the UserId only if the SecurityStamp is not set yet.
            string userId = context.Users.Where(x => x.Email == "n@m.com" && string.IsNullOrEmpty(x.SecurityStamp)).Select(x => x.Id).FirstOrDefault();

            //If the userId is not null, then the SecurityStamp needs updating.
            if (!string.IsNullOrEmpty(userId)) userManager.UpdateSecurityStamp(userId);

            //Repeat for next user.
            userId = context.Users.Where(x => x.Email == "s@r.com" && string.IsNullOrEmpty(x.SecurityStamp)).Select(x => x.Id).FirstOrDefault();

            if (!string.IsNullOrEmpty(userId)) userManager.UpdateSecurityStamp(userId);
        }
    }
}