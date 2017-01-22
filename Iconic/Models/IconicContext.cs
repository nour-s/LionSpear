using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace Iconic.Models
{
    public class IconicDbContext : DbContext
    {


        public DbSet<Movie> Movies { get; set; }

        public DbSet<Location> Locations { get; set; }
    }
}