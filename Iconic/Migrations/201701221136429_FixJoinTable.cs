namespace Iconic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class FixJoinTable : DbMigration
    {
        public override void Up()
        {
            RenameTable(name: "dbo.LocationMovies", newName: "MovieLocations");
            DropPrimaryKey("dbo.MovieLocations");
            AddPrimaryKey("dbo.MovieLocations", new[] { "Movie_Id", "Location_Id" });
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.MovieLocations");
            AddPrimaryKey("dbo.MovieLocations", new[] { "Location_Id", "Movie_Id" });
            RenameTable(name: "dbo.MovieLocations", newName: "LocationMovies");
        }
    }
}
