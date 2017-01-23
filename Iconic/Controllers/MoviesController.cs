using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Iconic.Models;
using System.Web;
using System.IO;
using System.Diagnostics;
 using System.Security.Claims;

namespace Iconic.Controllers
{
    public class MoviesController : ApiController
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        private const int pageSize = 20;

        public MoviesController()
        {
            db.Database.Log = x => { Debug.WriteLine(x); };
        }

        [AllowAnonymous]
        // GET: the main GET handler, it returns all the movies that match the passed parameters (if supplied).
        public IEnumerable<MovieViewModel> GetMovies(int page = 1, string name = null, string genre = null, string locName = null)
        {
            var list = db.Movies.OrderBy(o => o.Rating).Skip((page - 1) * pageSize).Take(pageSize);
            if (name != null)
                list = list.Where(w => w.Name == name);

            if (genre != null)
                list = list.Where(w => w.Genre.Contains(genre));

            if (locName != null)
                list = list.Include(m => m.Locations).Where(w => w.Locations.Any(a => a.Name == locName));

            var result = list.Select(s => new MovieViewModel() { Id = s.Id, Name = s.Name, Description = s.Description, Genre = s.Genre, Rating = s.Rating }).ToList();
            result.ForEach(f => f.Image = Url.Route("DefaultApi", new { controller = "Movies/Image", id = f.Id }));

            return result;
        }

        // GET: return the information of the passed movie id.
        [AllowAnonymous]
        [ResponseType(typeof(Movie))]
        public async Task<IHttpActionResult> GetMovie(int id)
        {
            Movie movie = await db.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            return Ok(movie);
        }

        // Get the image of the passed movie id
        [AllowAnonymous]
        [HttpGet, Route("api/movies/image/{movieId}")]
        public IHttpActionResult GetMovieImage(int movieId)
        {
            Movie movie = db.Movies.Find(movieId);
            if (movie == null)
            {
                return NotFound();
            }
            var result = Request.CreateResponse(HttpStatusCode.Gone);
            result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(movie.Image);
            result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpg");
            return ResponseMessage(result);
        }

        /// Return the bucket list of locations for the current user.
        /// The list contains the locations that either he added himself or were suggested by his friends.
        [Authorize]
        [HttpGet, Route("api/movies/bucketList")]
        public async Task<IHttpActionResult> GetBucketList()
        {
            //Get current user id.
            var userId = GetCurrentUserID();
            List<BucketListViewModel> result = null;

            //Since 'Where' method doesn't support async, I had to create a task manually that do the job async.
            await Task.Run(() =>
            {
                //Get the list of bucket list locations of the current user with required information.
                result = db.BucketLists.Include(i => i.SuggestedBy).Include(i => i.Location)
                .Where(w => w.OwnerId == userId)
                .Select(s => new BucketListViewModel
                {
                    Name = s.Location.Name,
                    Description = s.Location.Description,
                    LocationID = s.Id,
                    SuggestedBy = s.SuggestedBy.UserName,
                    Visited = s.Visited
                }).ToList();

                //Generating url cannot be done inside the select statement because it is parsed to an SQL statement.
                result.ForEach(f => f.Image = Url.Route("DefaultApi", new { controller = "Locations/Image", id = f.LocationID }));
            });

            return Ok(result);
        }

        // POST: link the list of passed locations to the movie with the passed id.
        [Authorize(Roles = "Admin")]
        [HttpPost, Route("api/movies/addlocation/{movieId}")]
        public async Task<IHttpActionResult> AddLocations(int movieId, params int[] locationIds)
        {
            var movie = db.Movies.Where(m => m.Id == movieId).Include(i => i.Locations).SingleOrDefault();

            if (movie == null)
            {
                return NotFound();
            }
            //TODO: Check if the locations exists ! 
            var locations = locationIds.Select(id => new Location { Id = id }).ToList();
            locations.ForEach(l => db.Entry(l).State = EntityState.Unchanged);

            movie.Locations.AddRange(locations);
            await db.SaveChangesAsync();

            return Ok("Locations added");
        }

        // PUT: update the movie information with the passed id.
        [ResponseType(typeof(void))]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> PutMovie(int id, Movie movie)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != movie.Id)
            {
                return BadRequest();
            }

            db.Entry(movie).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MovieExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // POST: add a movie to the database using the passed object.
        [ResponseType(typeof(Movie))]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> PostMovie(Movie movie)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Movies.Add(movie);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = movie.Id }, movie);
        }

        // DELETE: delete the movie with the passed id
        [ResponseType(typeof(Movie))]
        [Authorize(Roles = "Admin")]
        public async Task<IHttpActionResult> DeleteMovie(int id)
        {
            Movie movie = await db.Movies.FindAsync(id);
            if (movie == null)
            {
                return NotFound();
            }

            db.Movies.Remove(movie);
            await db.SaveChangesAsync();

            return Ok(movie);
        }

        // Upload an image for the passed movie id.
        [Authorize(Roles = "Admin")]
        [HttpPost, Route("api/movies/upload/{movieId}")]
        public IHttpActionResult Upload(int movieId)
        {
            Movie movie = db.Movies.Find(movieId);
            if (movie == null)
            {
                return NotFound();
            }

            if (HttpContext.Current.Request.Files.Count != 1)
                return BadRequest("You should submit a file.");

            HttpPostedFile hpf = HttpContext.Current.Request.Files[0];
            byte[] content = new byte[] { };
            byte[] fileData = null;
            using (var binaryReader = new BinaryReader(hpf.InputStream))
                fileData = binaryReader.ReadBytes(hpf.ContentLength);

            db.Entry(movie).State = EntityState.Modified;
            movie.Image = fileData;

            try
            {
                db.SaveChanges();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }
            return Ok("Image uploaded");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        // Check if there is a movie with 
        private bool MovieExists(int id)
        {
            return db.Movies.Any(e => e.Id == id);
        }

        private string GetCurrentUserID()
        {
            var identity = User.Identity as ClaimsIdentity;
            Claim identityClaim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            return identityClaim.Value;
        }
    }
}