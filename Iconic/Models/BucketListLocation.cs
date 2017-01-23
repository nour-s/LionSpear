using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Iconic.Models
{
    /// <summary>
    /// Represent a location that is in a bucket list of a user. it is a joining class between the user and the location.
    /// It contains information about this item of a bucket list.
    /// </summary>
    public class BucketListLocation
    {
        public int Id { get; set; }

        public bool Visited { get; set; }

        public ApplicationUser SuggestedBy { get; set; }

        public Location Location { get; set; }

        public string OwnerId { get; set; }
    }
}