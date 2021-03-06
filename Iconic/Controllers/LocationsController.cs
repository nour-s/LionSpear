﻿using System;
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
using System.Security.Claims;

namespace Iconic.Controllers
{
    public class LocationsController : ApiController
    {
        private IApplicationDbContext db;

        private const int pageSize = 20;
        public LocationsController(IApplicationDbContext context)
        {
            db = context ?? new ApplicationDbContext();
        }

        // GET: return paged locations list.
        public async Task<IHttpActionResult> GetLocations(int page = 1)
        {
            IQueryable<LocationViewModel> list = null;

            await Task.Run(() =>
            {
                list = db.Locations.OrderBy(o => o.Id).Skip((page - 1) * pageSize).Take(pageSize)
                .Select(s => new LocationViewModel() { Id = s.Id, Name = s.Name, Description = s.Description });
                list.ToList().ForEach(f => f.Image = Url.Route("DefaultApi", new { controller = "Locations", id = f.Id }));
            });

            return Ok(list);
        }

        [AllowAnonymous]
        public async Task<IHttpActionResult> GetMovieLocations(params int[] movieIds)
        {
            IQueryable<Movie> list = null;
            await Task.Run(() =>
            {
                list = db.Movies.Where(w => movieIds.Contains(w.Id));
            });

            var result = list.Select(s => new LocationViewModel() { Id = s.Id, Name = s.Name, Description = s.Description }).ToList();
            result.ForEach(f => f.Image = Url.Route("DefaultApi", new { controller = "Locations/Image", id = f.Id }));

            return Ok(result);
        }

        // GET: return the location with passed id
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


        // return the image of the location.
        [HttpGet, Route("api/locations/image/{locationid}")]
        [ResponseType(typeof(Movie))]
        public IHttpActionResult GetLocationImage(int locationid)
        {
            Location location = db.Locations.Find(locationid);
            if (location == null)
            {
                return NotFound();
            }

            var result = Request.CreateResponse(HttpStatusCode.OK);
            result.Content = new ByteArrayContent(location.Image);
            result.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpg");
            return ResponseMessage(result);
        }

        //Saves the location in the bucket list of the user whose id equals the passed userId.
        //If the userId is the same as the current, it will be added to his bucket list.
        [Authorize]
        [HttpPost, Route("api/locations/send/{locationId}/{userId}")]
        public async Task<IHttpActionResult> SendLocation(int locationId, string userId)
        {
            //Check if the location exists.
            var location = db.Locations.Find(locationId);
            if (location == null)
                return NotFound();

            //Check if the user exists.
            var user = db.Users.Where(w => w.Id == userId).Include(i => i.TravelBucketList).SingleOrDefault();
            if (user == null)
                return NotFound();

            var currentUser = GetCurrentUser();
            user.TravelBucketList.Add(new BucketListLocation { Location = location, OwnerId = user.Id, SuggestedBy = currentUser });

            await db.SaveChangesAsync();

            return Ok("Location sent.");
        }

        //Mark the location as visited.
        [Authorize]
        [HttpPost, Route("api/locations/setvisited/{locationId}")]
        public async Task<IHttpActionResult> SetVisited(int locationId)
        {
            var userId = GetCurrentUser().Id;
            var locations = db.BucketLists.Where(w => w.OwnerId == userId && w.Location.Id == locationId).ToList();
            locations.ForEach(f => f.Visited = true);
            await db.SaveChangesAsync();

            return Ok();
        }

        //Update the location with the passed id.
        // PUT: api/Locations/5
        [Authorize(Roles = "Admin")]
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

            if (!LocationExists(id))
            {
                return NotFound();
            }

            db.SetState(location, EntityState.Modified);

            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return StatusCode(HttpStatusCode.NoContent);
        }

        // Add the passed location to database.
        // POST: api/Locations
        [Authorize(Roles = "Admin")]
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

        // Delete the passed location from database.
        // DELETE: api/Locations/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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

            db.SetState(location, EntityState.Modified);
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

        private ApplicationUser GetCurrentUser()
        {
            var identity = User.Identity as ClaimsIdentity;
            Claim identityClaim = identity.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);

            return db.Users.FirstOrDefault(u => u.Id == identityClaim.Value);
        }

        private bool LocationExists(int id)
        {
            return db.Locations.Count(e => e.Id == id) > 0;
        }
    }
}