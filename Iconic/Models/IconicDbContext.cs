using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Data.Entity.Migrations;
using System.Web;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;

namespace Iconic.Models
{
    public class IconicDbContext : DbContext
    {
        static IconicDbContext()
        {
            Database.SetInitializer<IconicDbContext>(new IconicContextInitializer());
        }

        public DbSet<Movie> Movies { get; set; }

        public DbSet<Location> Locations { get; set; }

    }

    public class IconicContextInitializer : CreateDatabaseIfNotExists<IconicDbContext>
    {
        protected override void Seed(IconicDbContext context)
        {
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