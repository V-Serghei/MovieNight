using MovieNight.BusinessLogic.Core.ServiceApi;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.Entities.MovieM;

namespace MovieNight.BusinessLogic.Session.Service
{
    public class MovieService:MovieAPI,IMovie
    {
        public MovieTemplateInfE GetMovieInf(int? id)
        {
            return GetMovieFromDb( id);
        }

    }

    
}