using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Iconic.Models
{
    public class BucketListLocation
    {
        public int Id { get; set; }

        public bool Visited { get; set; }

        public ApplicationUser SuggestedBy { get; set; }

        public Location Location { get; set; }

        public ApplicationUser Owner { get; set; }


    }
}