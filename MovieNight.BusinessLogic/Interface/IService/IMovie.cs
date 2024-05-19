using System.Collections.Generic;
using System.Threading.Tasks;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.MovieM.SearchParam;
using MovieNight.Domain.Entities.PersonalP.PersonalPDb;
using MovieNight.Domain.Entities.Statistics;

namespace MovieNight.BusinessLogic.Interface.IService
{
    public interface IMovie
    {
        MovieTemplateInfE GetMovieInf(int? id);

        Task<BookmarkE> SetNewBookmark((int,int) movieid);
        bool GetInfBookmark((int?,int) movieid);
        bool GetInfBookmarkTimeOf((int, int) movieid);

        List<ListOfFilmsE> GetListPlain(int? userId); 
        BookmarkTimeOfE GetListBookmarksTimeOf(int? userId);
        Task<bool> DeleteBookmark((int Id, int movieId) valueTuple);
        float GetUserRating((int Id, int id) valueTuple);
        Task<bool> SetReteMovieAndView((int Id, int movieId, int rating) valueTuple);
        List<ViewingHistoryM> GetViewingList(int? userId);
        Task<IEnumerable<ViewingHistoryM>> GetNewViewList(ViewListSortCommandE transCommand);
        List<MovieTemplateInfE> GetListMovie(MovieCommandS movieSCommand);
        Task<RespToAddBookmarkTimeOf>  SetNewBookmarkTimeOf((int Id, int movieId) valueTuple);
        Task<bool> DeleteBookmarkTimeOf((int Id, int movieId) valueTuple);
        void BookmarkStatusCheck();
        Task<MovieTemplateInfE> GetRandomFilm();
        Task<InfMovieScoresE> GetInfOnFilmScores(int? userId);
        StatisticE GetDataStatisticPage(int? userId);
        Task<GenresDataStatistic> GetInfOnFilmGenres(int? userId);
        Task<GenresDataStatistic> GetInfOnFilmCountry(int userId);
        Task<RespAddViewListElDb> SetViewList((int? movieId, int? Id) valueTuple);
        void ClearBookmarks();
        List<ViewingHistoryM> GetBookmarkList(int? id);
        Task<List<BookmarkInfoE>> GetNewBookmarkTimeOfList(ListSortCommandE transCommand);
        List<BookmarkInfoE> GetListBookmarksTimeOfInfo(int? id);
    }
}