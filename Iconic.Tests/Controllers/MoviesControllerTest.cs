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
    public class MoviesControllerTest
    {
        [TestMethod]
        public void GetMovies()
        {
            TestApplicationDbContext db = new TestApplicationDbContext();
            db.Movies.AddRange(Enumerable.Repeat(new Movie()
            {
                Name = "Avatar",
                Description = "A parpalegic marine dispatched to the moon Pandora on a unique mission ",
                Genre = "Action, Adventure, Fantasy",
                Rating = 5,
            }, 20));

            MoviesController controller = GetMoviesController(db);

            // Act on Test  
            var response = controller.GetMovies();

            // Assert the result  
            var contentResult = response.Result as OkNegotiatedContentResult<List<MovieViewModel>>;

            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);
            Assert.AreEqual(db.Movies.Count(), contentResult.Content.Count);
        }

        [TestMethod]
        public void GetFilteredMoviesByTitle()
        {
            //Arrange for the test
            TestApplicationDbContext db = new TestApplicationDbContext();
            db.Movies.AddRange(Enumerable.Range(1, 20).Select(i => new Movie
            {
                Id = i,
                Name = string.Format("Movie{0}", i)
            }));

            MoviesController controller = GetMoviesController(db);

            //----------------- Test filter by genre -------------------- //
            var movieName = db.Movies.Find(new Random().Next(0, db.Movies.Count() - 1)).Name;
            // Act on Test  
            var response = controller.GetMovies(1, movieName); //Request all movies that have Genre1 

            // Assert the result
            var contentResult = response.Result as OkNegotiatedContentResult<List<MovieViewModel>>;

            //Check if there is a result, and the result returned content
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);

            //The ids that the controller "should" return
            var expectedIds = db.Movies.ToList().Where(w => w.Name == movieName).Select(s => s.Id);
            //The ids that the controller "actually" returned
            var actualIds = contentResult.Content.Select(s => s.Id);

            //Comare the list of ids
            Assert.IsTrue(expectedIds.SequenceEqual(actualIds));
        }

        [TestMethod]
        public void GetFilteredMoviesByGenre()
        {
            //Arrange for the test
            TestApplicationDbContext db = new TestApplicationDbContext();
            db.Movies.AddRange(Enumerable.Range(1, 20).Select(i => new Movie
            {
                Id = i,
                Genre = string.Format("Genre{0}", 5 - i % 5),
            }));

            MoviesController controller = GetMoviesController(db);

            //----------------- Test filter by genre -------------------- //
            var genreName = "Genre1";
            // Act on Test  
            var response = controller.GetMovies(1, null, genreName); //Request all movies that have Genre1 

            // Assert the result
            var contentResult = response.Result as OkNegotiatedContentResult<List<MovieViewModel>>;

            //Check if there is a result, and the result returned content
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);

            //The ids that the controller "should" return
            var expectedIds = db.Movies.ToList().Where(w => w.Genre == genreName).Select(s => s.Id);
            //The ids that the controller "actually" returned
            var actualIds = contentResult.Content.Select(s => s.Id);

            //Comare the list of ids
            Assert.IsTrue(expectedIds.SequenceEqual(actualIds));
        }

        [TestMethod]
        public void GetFilteredMoviesByLocation()
        {
            //Arrange for the test
            TestApplicationDbContext db = new TestApplicationDbContext();
            db.Movies.AddRange(Enumerable.Range(1, 20).Select(i => new Movie
            {
                Id = i,
                Locations = new List<Location> { new Location { Name = string.Format("Location{0}", 10 - i % 10) } }
            }));
            MoviesController controller = GetMoviesController(db);

            //----------------- Test filter by genre -------------------- //
            var locationName = "Location1";
            // Act on Test  
            var response = controller.GetMovies(1, null, null, locationName); //Request all movies that have Genre1 

            // Assert the result
            var contentResult = response.Result as OkNegotiatedContentResult<List<MovieViewModel>>;

            //Check if there is a result, and the result returned content
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);

            //The ids that the controller "should" return
            var expectedIds = db.Movies.ToList().Where(w => w.Locations.Any(l => l.Name == locationName)).Select(s => s.Id);
            //The ids that the controller "actually" returned
            var actualIds = contentResult.Content.Select(s => s.Id);

            //The ids that the controller "should" return
            //The ids that the controller "actually" returned
            actualIds = contentResult.Content.Select(s => s.Id);

            //Comare the list of ids
            Assert.IsTrue(expectedIds.SequenceEqual(actualIds));
        }

        [TestMethod]
        public void AddMovie()
        {
            //Arrange for the test
            TestApplicationDbContext db = new TestApplicationDbContext();
            Movie movie = new Movie { Id = 10 };

            MoviesController controller = GetMoviesController(db);
            var repsonse = controller.PostMovie(movie);

            var contentResult = repsonse.Result as CreatedAtRouteNegotiatedContentResult<Movie>;

            //Check if there is a result, and the result returned content
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);

            Assert.IsNotNull(db.Movies.Find(movie.Id));

        }

        [TestMethod]
        public void PutMovie()
        {
            //Arrange for the test
            TestApplicationDbContext db = new TestApplicationDbContext();
            db.Movies.AddRange(Enumerable.Range(1, 20).Select(i => new Movie
            {
                Id = i,
                Name = string.Format("Movie{0}", i)
            }));

            var movie = db.Movies.Find(new Random().Next(0, db.Movies.Count() - 1));
            movie.Name = "Updated";
            movie.Rating = 2;

            MoviesController controller = GetMoviesController(db);
            var repsonse = controller.PostMovie(movie);

            var contentResult = repsonse.Result as CreatedAtRouteNegotiatedContentResult<Movie>;

            //Check if there is a result, and the result returned content
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);

            Assert.IsTrue(contentResult.Content.Name == movie.Name && contentResult.Content.Rating == movie.Rating);

        }

        [TestMethod]
        public void DeleteMovie()
        {
            //Arrange for the test
            TestApplicationDbContext db = new TestApplicationDbContext();
            db.Movies.AddRange(Enumerable.Range(1, 20).Select(i => new Movie
            {
                Id = i,
                Name = string.Format("Movie{0}", i)
            }));

            int idToDelelte = new Random().Next(0, db.Movies.Count() - 1);

            MoviesController controller = GetMoviesController(db);
            var repsonse = controller.DeleteMovie(idToDelelte);

            var contentResult = repsonse.Result as OkNegotiatedContentResult<Movie>;

            //Check if there is a result, and the result returned content
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);

            Assert.IsNull(db.Movies.Find(idToDelelte));

        }

        [TestMethod]
        public void Get_ShouldFailWhenNotFound()
        {
            TestApplicationDbContext db = new TestApplicationDbContext();
            db.Movies.AddRange(Enumerable.Repeat(new Movie()
            {
                Name = "Avatar",
                Description = "A parpalegic marine dispatched to the moon Pandora on a unique mission ",
                Genre = "Action, Adventure, Fantasy",
                Rating = 5,
            }, 20));

            MoviesController controller = GetMoviesController(db);

            // Act on Test  
            var response = controller.GetMovie(int.MaxValue);

            // Assert the result  
            var contentResult = response.Result as NotFoundResult;

            Assert.IsNotNull(contentResult);
        }

        [TestMethod]
        public void Put_ShouldFailWhenNotFoundID()
        {
            TestApplicationDbContext db = new TestApplicationDbContext();
            Movie movie = new Movie { Id = int.MaxValue };

            MoviesController controller = GetMoviesController(db);

            // Act on Test  
            var response = controller.PutMovie(movie.Id, movie);

            // Assert the result  
            var contentResult = response.Result as NotFoundResult;

            Assert.IsNotNull(contentResult);
        }

        [TestMethod]
        public void AddLocationsToMovie()
        {
            //Arrange for the test
            TestApplicationDbContext db = new TestApplicationDbContext();
            Movie movie = new Movie { Id = 1, Locations = new List<Location>() };
            db.Movies.Add(movie);
            db.Locations.AddRange(Enumerable.Range(1, 3).Select(i => new Location() { Id = i }));

            MoviesController controller = GetMoviesController(db);
            var repsonse = controller.AddLocations(movie.Id, db.Locations.Select(s => s.Id).ToArray());

            var contentResult = repsonse.Result as OkNegotiatedContentResult<string>;

            //Check if there is a result, and the result returned content
            Assert.IsNotNull(contentResult);
            Assert.IsNotNull(contentResult.Content);

            var actual = db.Movies.First().Locations.Select(s => s.Id);
            var expected = db.Locations.Select(s => s.Id).ToArray();
            Assert.IsTrue(expected.SequenceEqual(expected));
        }

        /// <summary>
        /// Return a ready instance of MoviesController.
        /// </summary>
        /// <param name="db"></param>
        /// <returns></returns>
        private MoviesController GetMoviesController(TestApplicationDbContext db)
        {
            MoviesController controller = new MoviesController(db);
            var routeCollection = new HttpRouteCollection();
            routeCollection.MapHttpRoute("DefaultApi", "api/{controller}/");

            var httpConfig = new HttpConfiguration(routeCollection);
            var request = new HttpRequestMessage(HttpMethod.Get, "movies");

            controller.Configuration = httpConfig;
            controller.Request = request;
            return controller;
        }
    }
}
