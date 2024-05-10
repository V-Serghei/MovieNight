using System.Collections.Generic;
using System.Threading.Tasks;
using MovieNight.BusinessLogic.Core.ServiceApi;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.MovieM.SearchParam;
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

        public async Task<bool> DeleteBookmark((int Id, int movieId) valueTuple)
        {
            return await DeleteBookmarkDb(valueTuple);
        }

        public float GetUserRating((int Id, int id) valueTuple)
        {
            return GetUserRatingDb(valueTuple);

        }

        public async Task<bool> SetReteMovieAndView((int Id, int movieId, int rating) valueTuple)
        {
            return await SetReteMovieAndViewDb(valueTuple);
        }

        public List<ViewingHistoryM> GetViewingList(int? userId)
        {
            return GetViewingListDb(userId);
        }

        public async Task<IEnumerable<ViewingHistoryM>> GetNewViewList(ViewListSortCommandE transCommand)
        {
            return  await GetNewViewListDb(transCommand);
        }

        public List<MovieTemplateInfE> GetListMovie(MovieCommandS movieSCommand)
        {
            return GetListMovieDb(movieSCommand);
        }
    }

    
}