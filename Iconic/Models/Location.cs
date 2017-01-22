using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Iconic.Models
{
    /// <summary>
    /// Represents an iconic location of a movie.
    /// </summary>
    public class Location
    {
        /// <summary>
        /// Location identifier.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The name of the location.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The description of the location.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// An image that shows the location of a movie.
        /// </summary>
        public byte[] Image { get; set; }

    }
}
