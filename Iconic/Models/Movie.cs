using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Iconic.Models
{
    /// <summary>
    /// The movie details that will be selected to visit its iconic locations.
    /// </summary>
    public class Movie
    {
        public int Id { get; set; }

        /// <summary>
        /// The name of the movie, should be 255 maximum.
        /// </summary>
        [MaxLength(255, ErrorMessage = "Name cannot be more than 255 characters")]
        public string Name { get; set; }

        /// <summary>
        /// The description of the movie.
        /// </summary>
        [Required]
        public string Description { get; set; }

        /// <summary>
        /// Comma separated list of genres of this movie.
        /// </summary>
        public string Genre { get; set; }

        /// <summary>
        /// The poster of the movie.
        /// </summary>
        public byte[] Image { get; set; }

        /// <summary>
        /// The rating of the movie which represents how popular is the movie.
        /// </summary>
        public int Rating { get; set; }

        /// <summary>
        /// The locations where the movie is produced
        /// </summary>
        public List<Location> Locations { get; set; }
    }
}