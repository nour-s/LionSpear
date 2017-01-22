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

namespace Iconic.Controllers
{
    public class MoviesController : ApiController
    {
        private IconicDbContext db = new IconicDbContext();

        private const int pageSize = 20;

        public MoviesController()
        {
            db.Database.Log = x => { Debug.WriteLine(x); };
        }

        // GET: api/Movie
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
            result.ForEach(f => f.Image = Url.Route("DefaultApi", new { controller = "Movie", id = f.Id }));

            return result;
        }

        // GET: api/Movies/5
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

        // POST: api/Movies
        [HttpPost, Route("api/movies/addlocation/{movieId}")]
        public async Task<IHttpActionResult> AddLocations(int movieId, params int[] locationIds)
        {
            var movie = db.Movies.Where(m => m.Id == movieId).Include(i => i.Locations).SingleOrDefault();

            if (movie == null)
            {
                return NotFound();
            }

            var locations = locationIds.Select(id => new Location { Id = id }).ToList();
            locations.ForEach(l => db.Entry(l).State = EntityState.Unchanged);

            movie.Locations.AddRange(locations);
            await db.SaveChangesAsync();

            return Ok("Locations added");
        }

        // PUT: api/Movies/5
        [ResponseType(typeof(void))]
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

        // POST: api/Movies
        [ResponseType(typeof(Movie))]
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

        // DELETE: api/Movies/5
        [ResponseType(typeof(Movie))]
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

        private bool MovieExists(int id)
        {
            return db.Movies.Count(e => e.Id == id) > 0;
        }
    }
}