﻿using System.Collections.Generic;
using System.Threading.Tasks;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.MovieM.SearchParam;
using MovieNight.Domain.Entities.PersonalP.PersonalPDb;
using MovieNight.Domain.Entities.Review;

namespace MovieNight.BusinessLogic.Interface.IService
{
    public interface IMovie
    {
        MovieTemplateInfE GetMovieInf(int? id);

        Task<BookmarkE> SetNewBookmark((int,int) movieid);
        bool GetInfBookmark((int,int) movieid);

        List<ListOfFilmsE> GetListPlain(int? userId);
        Task<bool> DeleteBookmark((int Id, int movieId) valueTuple);
        float GetUserRating((int Id, int id) valueTuple);
        Task<bool> SetReteMovieAndView((int Id, int movieId, int rating) valueTuple);
        List<ViewingHistoryM> GetViewingList(int? userId);
        Task<IEnumerable<ViewingHistoryM>> GetNewViewList(ViewListSortCommandE transCommand);
        List<MovieTemplateInfE> GetListMovie(MovieCommandS movieSCommand);
        List<ReviewE> getListOfReviews(int? filmId);
        bool setNewReview(ReviewE reviewE);
        int? DeleteReview(int? reviewE);
    }
}