﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MovieNight.BusinessLogic.Core.ServiceApi;
using MovieNight.BusinessLogic.Interface.IService;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.MovieM.SearchParam;
using MovieNight.Domain.Entities.PersonalP.PersonalPDb;
using MovieNight.Domain.Entities.Statistics;

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

        public bool GetInfBookmark((int?,int) movieid)
        {
            return GetInfBookmarkDb(movieid);
        }
        public bool GetInfBookmarkTimeOf((int,int) movieid)
        {
            return GetInfBookmarkTimeOfDb(movieid);
        }

        public List<ListOfFilmsE> GetListPlain(int? userId)
        {
            return GetListPlainDb(userId);
        }

        public BookmarkTimeOfE GetListBookmarksTimeOf(int? userId)
        {
            return GetListBookmarksTimeOfDb(userId);
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

        public async Task<RespToAddBookmarkTimeOf> SetNewBookmarkTimeOf((int Id, int movieId) valueTuple)
        {
            return await SetNewBookmarkTimeOfDb(valueTuple);
        }

        public async Task<bool> DeleteBookmarkTimeOf((int Id, int movieId) valueTuple)
        {
            return await DeleteBookmarkTimeOfDb(valueTuple);
        }

        public void BookmarkStatusCheck()
        {
            BookmarkStatusCheckDb();
        }

        public async Task<MovieTemplateInfE> GetRandomFilm()
        {
            return await GetRandomFilmDb();
        }

        public async Task<InfMovieScoresE> GetInfOnFilmScores(int? userId)
        {
            return await GetInfOnFilmScoresDb(userId);
        }

        public StatisticE GetDataStatisticPage(int? userId)
        {
            return GetDataStatisticPageApi(userId);
        }

        public async Task<GenresDataStatistic> GetInfOnFilmGenres(int? userId)
        {
            return await GetInfOnFilmGenresDb(userId);
        }

        public async Task<GenresDataStatistic> GetInfOnFilmCountry(int userId)
        {
            return await GetInfOnFilmCountryDb(userId);
        }

        public async Task<RespAddViewListElDb> SetViewList((int? movieId, int? Id) valueTuple)
        {
            return await SetViewListDb(valueTuple);
        }

        public void ClearBookmarks()
        {
            ClearBookmarksDb();
        }

        public List<ViewingHistoryM> GetBookmarkList(int? id)
        {
            return GetBookmarkTimeOfListDb(id);
        }

        public async Task<IEnumerable<ViewingHistoryM>> GetNewBookmarkTimeOfList(ViewListSortCommandE transCommand)
        {
            return await GetNewBookmarkTimeOfListDb(transCommand);
        }
    }

    
}