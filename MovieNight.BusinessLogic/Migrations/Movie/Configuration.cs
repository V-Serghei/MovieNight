namespace MovieNight.BusinessLogic.Migrations.Movie
{
    using MovieNight.BusinessLogic.DBModel;
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<MovieNight.BusinessLogic.DBModel.MovieContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = true;
            MigrationsDirectory = @"Migrations\Movie";
        }

        protected override void Seed(MovieContext context)
        {
        }
    }
}
