using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Iconic.Controllers;
using System.Net.Http;
using System.Web.Http;
using Iconic.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http.Results;
using System.Web.Http.Controllers;
using System.Web;
using System.IO;

namespace Iconic.Tests.Controllers
{
    [TestClass]
    public class LocationsControllerTest
    {
        [TestMethod]
        public void GetLocations()
        {
            TestApplicationDbContext db = new TestApplicationDbContext();
            db.Locations.AddRange(Enumerable.Repeat(new Location()
            {
                Name = "Avatar",
                Description = "A parpalegic marine dispatched to the moon Pandora on a unique mission "
            }, 20));

            LocationsController controller = GetLocationsController(db);

            // Act on Test  
            var response = controller.GetLocations();

            // Assert the result  
            var contentResult = response.Result as OkNegotiatedContentResult<IQueryable<LocationViewModel>>;

            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(db.Locations.Count(), contentResult.Content.Count());
        }

        [TestMethod]
        public void AddLocation()
        {
            //Arrange for the test
            TestApplicationDbContext db = new TestApplicationDbContext();
            Location location = new Location { Id = 10 };

            LocationsController controller = GetLocationsController(db);
            var repsonse = controller.PostLocation(location);

            var contentResult = repsonse.Result as CreatedAtRouteNegotiatedContentResult<Location>;

            //Check if there is a result, and the result returned content
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);

            Assert.IsNotNull(db.Locations.Find(location.Id));

        }

        [TestMethod]
        public void PutLocation()
        {
            //Arrange for the test
            TestApplicationDbContext db = new TestApplicationDbContext();
            db.Locations.AddRange(Enumerable.Range(1, 20).Select(i => new Location
            {
                Id = i,
                Name = string.Format("Location{0}", i)
            }));

            var location = db.Locations.Find(new Random().Next(0, db.Locations.Count() - 1));
            location.Name = "Updated";

            LocationsController controller = GetLocationsController(db);
            var repsonse = controller.PostLocation(location);

            var contentResult = repsonse.Result as CreatedAtRouteNegotiatedContentResult<Location>;

            //Check if there is a result, and the result returned content
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);

            Assert.IsTrue(contentResult.Content.Name == location.Name);

        }

        [TestMethod]
        public void DeleteLocation()
        {
            //Arrange for the test
            TestApplicationDbContext db = new TestApplicationDbContext();
            db.Locations.AddRange(Enumerable.Range(1, 20).Select(i => new Location
            {
                Id = i,
                Name = string.Format("Location{0}", i)
            }));

            int idToDelelte = new Random().Next(0, db.Locations.Count() - 1);

            LocationsController controller = GetLocationsController(db);
            var repsonse = controller.DeleteLocation(idToDelelte);

            var contentResult = repsonse.Result as OkNegotiatedContentResult<Location>;

            //Check if there is a result, and the result returned content
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);

            Assert.IsNull(db.Locations.Find(idToDelelte));

        }

        [TestMethod]
        public void Get_ShouldFailWhenNotFound()
        {
            TestApplicationDbContext db = new TestApplicationDbContext();
            db.Locations.AddRange(Enumerable.Repeat(new Location()
            {
                Name = "Avatar",
                Description = "A parpalegic marine dispatched to the moon Pandora on a unique mission ",
            }, 20));

            LocationsController controller = GetLocationsController(db);

            // Act on Test  
            var response = controller.GetLocation(int.MaxValue);

            // Assert the result  
            var contentResult = response.Result as NotFoundResult;

            Assert.IsNotNull(contentResult);
        }

        [TestMethod]
        public void Put_ShouldFailWhenNotFoundID()
        {
            TestApplicationDbContext db = new TestApplicationDbContext();
            Location location = new Location { Id = int.MaxValue };

            LocationsController controller = GetLocationsController(db);

            // Act on Test  
            var response = controller.PutLocation(location.Id, location);

            // Assert the result  
            var contentResult = response.Result as NotFoundResult;

            Assert.IsNotNull(contentResult);
        }
        
        /// <summary>
        /// Return a ready instance of LocationsController.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        private LocationsController GetLocationsController(TestApplicationDbContext db)
        {
            LocationsController controller = new LocationsController(db);
            var routeCollection = new HttpRouteCollection();
            routeCollection.MapHttpRoute("DefaultApi", "api/{controller}/");

            var httpConfig = new HttpConfiguration(routeCollection);
            var request = new HttpRequestMessage(HttpMethod.Get, "locations");

            controller.Configuration = httpConfig;
            controller.Request = request;
            return controller;
        }
    }
}
