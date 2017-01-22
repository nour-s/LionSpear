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

namespace Iconic.Controllers
{
    public class LocationsController : ApiController
    {
        private IconicDbContext db = new IconicDbContext();

        private const int pageSize = 20;

        // GET: api/Locations
        public IEnumerable<LocationViewModel> GetLocations(int page = 1)
        {
            var list = db.Locations.OrderBy(o => o.Id).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(s => new LocationViewModel() { Id = s.Id, Name = s.Name, Description = s.Description }).ToList();
            list.ForEach(f => f.Image = Url.Route("DefaultApi", new { controller = "Locations", id = f.Id }));
            return list;
        }


        // GET: api/Locations/5
        [ResponseType(typeof(Location))]
        public async Task<IHttpActionResult> GetLocation(int id)
        {
            Location location = await db.Locations.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            return Ok(location);
        }


        [HttpGet, Route("api/locations/image/{locationid}")]
        [ResponseType(typeof(Movie))]
        public IHttpActionResult GetLocationImage(int locationid)
        {
            Location location = db.Locations.Find(locationid);
            if (location == null)
            {
                return NotFound();
            }
            var result = Request.CreateResponse(HttpStatusCode.Gone);
            result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(location.Image);
            result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpg");
            return ResponseMessage(result);
        }

        // PUT: api/Locations/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutLocation(int id, Location location)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != location.Id)
            {
                return BadRequest();
            }

            db.Entry(location).State = EntityState.Modified;

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LocationExists(id))
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

        // POST: api/Locations
        [ResponseType(typeof(Location))]
        public async Task<IHttpActionResult> PostLocation(Location location)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            db.Locations.Add(location);
            await db.SaveChangesAsync();

            return CreatedAtRoute("DefaultApi", new { id = location.Id }, location);
        }

        // DELETE: api/Locations/5
        [ResponseType(typeof(Location))]
        public async Task<IHttpActionResult> DeleteLocation(int id)
        {
            Location location = await db.Locations.FindAsync(id);
            if (location == null)
            {
                return NotFound();
            }

            db.Locations.Remove(location);
            await db.SaveChangesAsync();

            return Ok(location);
        }

        // POST Upload image for a location with the passed id.
        [HttpPost, Route("api/locations/upload/{locationId}")]
        public IHttpActionResult Upload(int locationid)
        {
            Location location = db.Locations.Find(locationid);
            if (location == null)
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

            db.Entry(location).State = EntityState.Modified;
            location.Image = fileData;

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

        private bool LocationExists(int id)
        {
            return db.Locations.Count(e => e.Id == id) > 0;
        }
    }
}