using System.Collections.Generic;

namespace MovieNight.Web.Models.Movie.SearchPages
{
    public class FilmsListModel
    {
        public FilmCommandSort CommandSort { get; set; }
        public List<MovieTemplateInfModel> ListFilm { get; set; }
        
    }
}