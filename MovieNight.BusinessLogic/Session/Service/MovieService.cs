using System.Collections.Generic;
using System.Threading.Tasks;
using MovieNight.BusinessLogic.Core.ServiceApi;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.PersonalP.PersonalPDb;

namespace MovieNight.BusinessLogic.Session.Service
{
    public class MovieService:MovieAPI,IMovie
    {
        public MovieTemplateInfE GetMovieInf(int? id)
        {
            return GetMovieFromDb( id);
        }

        public async Task<BookmarkE> SetNewBookmark((int, int) idAdd)
        {
            return await SetNewBookmarkDb(idAdd);
        }

        public bool GetInfBookmark((int,int) movieid)
        {
            return GetInfBookmarkDb(movieid);
        }

        public List<ListOfFilmsE> GetListPlain(int? userId)
        {
            return GetListPlainDb(userId);
        }

      
        
        

    }

    
}