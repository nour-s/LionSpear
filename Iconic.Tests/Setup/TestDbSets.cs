using Iconic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iconic.Tests
{
    /// <summary>
    /// Mock implmentation of the DbSet that will contains a collection of movies.
    /// </summary>
    class TestMovieDbSet : MockDbSet<Movie>
    {
        public override Movie Find(params object[] keyValues)
        {
            return this.SingleOrDefault(movie => movie.Id == (int)keyValues.Single());
        }

        public override Task<Movie> FindAsync(params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }
    }

    /// <summary>
    /// Mock implmentation of the DbSet that will contains a collection of locations.
    /// </summary>
    class TestLocationDbSet : MockDbSet<Location>
    {
        public override Location Find(params object[] keyValues)
        {
            return this.SingleOrDefault(location => location.Id == (int)keyValues.Single());
        }

        public override Task<Location> FindAsync(params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }
    }

    /// <summary>
    /// Mock implmentation of the DbSet that will contains a collection of bucket list locations.
    /// </summary>
    class TestBucketListLocationDbSet : MockDbSet<BucketListLocation>
    {
        public override BucketListLocation Find(params object[] keyValues)
        {
            return this.SingleOrDefault(bklocation => bklocation.Id == (int)keyValues.Single());
        }
    }
}
