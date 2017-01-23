using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Iconic.Models
{
    /// <summary>
    /// Contains the main required assets to be used by the controllers to access database.
    /// </summary>
    public interface IApplicationDbContext : IDisposable
    {
        /// <summary>
        /// Represents the collection of all movies in the context.
        /// </summary>
        DbSet<Movie> Movies { get; set; }

        /// <summary>
        /// Represents the collection of all locations in the context.
        /// </summary>
        DbSet<Location> Locations { get; set; }

        /// <summary>
        /// Represents the collection of all bucket list locations in the context.
        /// </summary
        DbSet<BucketListLocation> BucketLists { get; set; }

        int SaveChanges();

        Task<int> SaveChangesAsync();

        /// <summary>
        /// Set the entity state in the context (uses entity(**).Status internally)
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="state"></param>
        void SetState<T>(T item, EntityState state) where T : class;
    }
}