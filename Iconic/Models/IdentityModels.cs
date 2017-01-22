using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Collections.Generic;
using System;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace Iconic.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public List<BucketListLocation> TravelBucketList { get; set; }

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
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationUser>().HasMany(a => a.TravelBucketList).WithRequired(a => a.Owner);
        }
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

        public DbSet<Movie> Movies { get; set; }

        public DbSet<Location> Locations { get; set; }

    }

    public class IdentitySeeder : CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            base.Seed(context);

            //The UserStore is ASP Identity's data layer. Wrap context with the UserStore.
            UserStore<ApplicationUser> userStore = new UserStore<ApplicationUser>(context);

            UserManager<ApplicationUser> userManager = new UserManager<ApplicationUser>(userStore);

            var users = new List<ApplicationUser> {  new ApplicationUser() { Email = "n@m.com", UserName = "nour", PasswordHash = new PasswordHasher().HashPassword("!@#123aA") },
                new ApplicationUser() { Email = "s@r.com", UserName = "user1", PasswordHash = new PasswordHasher().HashPassword("!@#123aA")} };

            //Add or Update the initial Users into the database as normal.
            context.Users.AddOrUpdate(x => x.Email,  //Using Email as the Unique Key: If a record exists with the same email, AddOrUpdate skips it.
                users.ToArray());

            context.SaveChanges();

            //Get the UserId only if the SecurityStamp is not set yet.
            string userId = context.Users.Where(x => x.Email == "n@m.com" && string.IsNullOrEmpty(x.SecurityStamp)).Select(x => x.Id).FirstOrDefault();

            //If the userId is not null, then the SecurityStamp needs updating.
            if (!string.IsNullOrEmpty(userId)) userManager.UpdateSecurityStamp(userId);

            //Repeat for next user.
            userId = context.Users.Where(x => x.Email == "s@r.com" && string.IsNullOrEmpty(x.SecurityStamp)).Select(x => x.Id).FirstOrDefault();

            if (!string.IsNullOrEmpty(userId)) userManager.UpdateSecurityStamp(userId);

            var locations = new List<Location> { new Location() { Name = "New York", Description = "Big city in USA", Image = GetRandomImage() },
                new Location() { Name = "Los Angeles", Description = "Another city in USA", Image = GetRandomImage() },
                new Location() { Name = "Paris", Description = "A city of wonders", Image = GetRandomImage() },
                new Location() { Name = "Madrid", Description = "Beatiful city in Spain", Image = GetRandomImage() },
                new Location() { Name = "Venice", Description = "City in sea", Image = GetRandomImage() }};

            context.Locations.AddOrUpdate(x => x.Name, locations.ToArray());

            context.SaveChanges();
            var movies = new List<Movie> { new Movie()
            {
                Name = "Avatar",
                Description = "A paraplegic marine dispatched to the moon Pandora on a unique mission ",
                Genre = "Action, Adventure, Fantasy",
                Rating = 5,
                Image = GetRandomImage(),
            },
                new Movie()
                {
                    Name = "Matrix",
                    Description = "A computer hacker learns from mysterious rebels about the true nature of his reality and his role in the war against its controllers",
                    Genre = "Action, Sci-Fi",
                    Rating = 5,
                    Image = GetRandomImage()
                },
                new Movie()
                {
                    Name = "X-Men",
                    Description = "Two mutants come to a private academy for their kind whose resident superhero team must oppose a terrorist organization with similar powers.",
                    Genre = " Action, Adventure, Sci-Fi",
                    Rating = 4,
                    Image = GetRandomImage()
                },
                new Movie()
                {
                    Name = "Fast & Furious",
                    Description = "When a mysterious woman seduces Dom into the world of crime and a betrayal of those closest to him, the crew face trials that will test them as never before.",
                    Genre = "Action, Crime, Thriller",
                    Rating = 4,
                    Image = GetRandomImage()
                },
                new Movie()
                {
                    Name = "Finding Dory",
                    Description = "The friendly but forgetful blue tang fish begins a search for her long-lost parents",
                    Genre = "Drama, Thriller, Mystery",
                    Rating = 3,
                    Image = GetRandomImage()
                }};

            context.Movies.AddOrUpdate(x => x.Name, movies.ToArray());

            var rnd = new Random().Next(0, locations.Count);
            foreach (var m in movies)
            {
                m.Locations = locations.Skip(rnd).Take(rnd).ToList();
                rnd = new Random().Next(0, locations.Count);
            }
            users[0].TravelBucketList = new List<BucketListLocation>();
            users[0].TravelBucketList.Add(new BucketListLocation { Location = locations[0], Owner = users[0] });
            users[0].TravelBucketList.Add(new BucketListLocation { Location = locations[1], Owner = users[0], SuggestedBy = users[1] });

            users[1].TravelBucketList = new List<BucketListLocation>();
            users[1].TravelBucketList.Add(new BucketListLocation { Location = locations[0], Owner = users[1] });
            users[1].TravelBucketList.Add(new BucketListLocation { Location = locations[1], Owner = users[1], SuggestedBy = users[0] });

            context.SaveChanges();
        }

        private byte[] GetRandomImage()
        {
            Bitmap finalBmp = new Bitmap(2, 2);
            for (int x = 0; x < 2; x++)
            {
                for (int y = 0; y < 2; y++)
                {
                    int num = new Random().Next(0, 256);
                    finalBmp.SetPixel(x, y, Color.FromArgb(255, num, num, num));
                }
            }

            MemoryStream ms = new MemoryStream();
            finalBmp.Save(ms, ImageFormat.Jpeg);
            return ms.ToArray();
        }
    }
}
