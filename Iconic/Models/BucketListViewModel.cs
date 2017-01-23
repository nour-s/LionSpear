using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Iconic.Models
{
    public class BucketListViewModel
    {
        public int LocationID { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Image { get; set; }

        public bool Visited { get; set; }

        public string SuggestedBy { get; set; }

    }
}