using Iconic.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iconic.Tests
{
    /// <summary>
    /// Mock implementation of IApplicationDbContext which will be used by the unit testing classes
    /// Act like normal db context but uses the memory to store the data instead of database.
    /// </summary>
    class TestApplicationDbContext : IApplicationDbContext
    {
        public TestApplicationDbContext()
        {
            //Initializing the dbset properties with a mock implementation.
            this.Movies  = new TestMovieDbSet();
            this.Locations = new TestLocationDbSet();
            this.BucketLists = new TestBucketListLocationDbSet();
        }

        public DbSet<Movie> Movies { get; set; }

        public DbSet<Location> Locations { get; set; }

        public DbSet<BucketListLocation> BucketLists { get; set; }

        /// <summary>
        /// Do nothing, since there is no database.
        /// </summary>
        public int SaveChanges() { return 0; }

        /// <summary>
        /// Do nothing, since there is no database.
        /// </summary>
        public Task<int> SaveChangesAsync()
        {
            return Task.FromResult(0);
        }

        public void SetState<T>(T item, EntityState state) where T : class
        {
            //Do nothing
        }

        public void Dispose() { }
    }
}
