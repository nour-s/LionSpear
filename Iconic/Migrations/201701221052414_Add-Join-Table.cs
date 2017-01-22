namespace Iconic.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddJoinTable : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Locations", "Movie_Id", "dbo.Movies");
            DropIndex("dbo.Locations", new[] { "Movie_Id" });
            CreateTable(
                "dbo.LocationMovies",
                c => new
                    {
                        Location_Id = c.Int(nullable: false),
                        Movie_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.Location_Id, t.Movie_Id })
                .ForeignKey("dbo.Locations", t => t.Location_Id, cascadeDelete: true)
                .ForeignKey("dbo.Movies", t => t.Movie_Id, cascadeDelete: true)
                .Index(t => t.Location_Id)
                .Index(t => t.Movie_Id);
            
            DropColumn("dbo.Locations", "Movie_Id");
        }
        
        public override void Down()
        {
            AddColumn("dbo.Locations", "Movie_Id", c => c.Int());
            DropForeignKey("dbo.LocationMovies", "Movie_Id", "dbo.Movies");
            DropForeignKey("dbo.LocationMovies", "Location_Id", "dbo.Locations");
            DropIndex("dbo.LocationMovies", new[] { "Movie_Id" });
            DropIndex("dbo.LocationMovies", new[] { "Location_Id" });
            DropTable("dbo.LocationMovies");
            CreateIndex("dbo.Locations", "Movie_Id");
            AddForeignKey("dbo.Locations", "Movie_Id", "dbo.Movies", "Id");
        }
    }
}
