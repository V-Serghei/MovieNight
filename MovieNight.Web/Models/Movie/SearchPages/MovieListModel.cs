using System.Collections.Generic;

namespace MovieNight.Web.Models.Movie.SearchPages
{
    public class MovieListModel
    {
        public FilmCommandSort CommandSort { get; set; }
        public List<MovieTemplateInfModel> ListFilm { get; set; }
        
    }
}