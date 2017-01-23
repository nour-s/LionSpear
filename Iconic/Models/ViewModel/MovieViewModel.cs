using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Iconic.Models
{
    public class MovieViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Genre { get; set; }

        public string Image { get; set; }

        public int Rating { get; set; }

    }
}