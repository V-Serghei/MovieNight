﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using AutoMapper;
using MovieNight.BusinessLogic.DBModel;
using MovieNight.BusinessLogic.Migrations.User;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.MovieM.EfDbEntities;
using MovieNight.Domain.Entities.MovieM.SearchParam;
using MovieNight.Domain.Entities.PersonalP.PersonalPDb;
using Newtonsoft.Json;
using EntityState = System.Data.Entity.EntityState;

namespace MovieNight.BusinessLogic.Core.ServiceApi
{
    public class MovieAPI
    {
        //var op = ReadMoviesFromJson("D:\\web project\\Movie\\MovieNight\\MovieNight.BusinessLogic\\DBModel\\Seed\\SeedData.json");

        private IMapper MapperFilm{ get; set; }
        private IMapper MapperFact{ get; set; }
        private IMapper MapperCast{ get; set; }
        private IMapper MapperCard{ get; set; }
        
        private IMapper MapperViewList { get; set; }

        private List<IMapper> LMapper => GetMappersSettings();


        //If you’re back to the meper again, remember,
        //check first if you called in your method where you want to use it,
        //the method itself with all the settings of the meper
        private List<IMapper> GetMappersSettings()  
        {
            var conf = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<MovieDbTable, MovieTemplateInfE>()
                    .ForMember(dest => dest.Genre,
                        opt => opt.MapFrom(src => JsonConvert.DeserializeObject<List<string>>(src.Genres)))
                    .ForMember(dest => dest.DurationJ,
                        opt => opt.MapFrom(src => src.Duration.ToString(CultureInfo.InvariantCulture)))
                    .ForMember(dist =>
                        dist.InterestingFacts, src =>
                        src.Ignore())
                    .ForMember(dist =>
                        dist.CastMembers, src => src.Ignore())
                    .ForMember(dist =>
                        dist.MovieCards, src =>
                        src.Ignore());
                cfg.CreateMap<MovieTemplateInfE, MovieDbTable>()
                    .ForMember(dest => dest.Genres, opt => opt.MapFrom(src => JsonConvert.SerializeObject(src.Genre)))
                    .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => DateTime.Parse(src.DurationJ)))
                    .ForMember(dist =>
                        dist.InterestingFacts, src =>
                        src.Ignore())
                    .ForMember(dist =>
                        dist.CastMembers, src => src.Ignore())
                    .ForMember(dist =>
                        dist.MovieCards, src =>
                        src.Ignore())
                    .ForMember(dist => dist.BookmarkDbTables, src => src.Ignore())
                    .ForMember(dist => dist.ViewListEntries, src => src.Ignore());
            });
            MapperFilm = conf.CreateMapper();
            var confInterFact = new MapperConfiguration(config =>
            {
                config.CreateMap<InterestingFactE, InterestingFactDbTable>()
                    .ForMember(dst => dst.Id, src =>
                        src.Ignore())
                    .ForMember(dst => dst.MovieId, src =>
                        src.Ignore());
                config.CreateMap<InterestingFactDbTable, InterestingFactE>();
            });
            
            MapperFact = confInterFact.CreateMapper();
            var confCast = new MapperConfiguration(config =>
            {
                config.CreateMap<CastMemberE, CastMemDbTable>()
                    .ForMember(dst => dst.Id, src =>
                        src.Ignore())
                    .ForMember(dst => dst.Movies, src =>
                        src.Ignore());
                config.CreateMap<CastMemDbTable, CastMemberE>();
            });
            MapperCast = confCast.CreateMapper();
            var confCard = new MapperConfiguration(config =>
            {
                config.CreateMap<MovieCardE, MovieCardDbTable>()
                    .ForMember(dst => dst.Id, src =>
                        src.Ignore())
                    .ForMember(dst => dst.MovieId, src =>
                        src.Ignore());
                config.CreateMap<MovieCardDbTable, MovieCardE>();
            });
            MapperCard = confCard.CreateMapper();

            var configV = new MapperConfiguration(confV =>
            {
                confV.CreateMap<ViewingHistoryM, ViewListDbTable>()
                    .ForPath(dest => dest.Movie.MovieNightGrade, opt =>
                        opt.MapFrom(src => src.MovieNightGrade))
                    .ForPath(dist => dist.Movie.ProductionYear, opt =>
                        opt.MapFrom(src => src.YearOfRelease));
                confV.CreateMap<ViewListDbTable, ViewingHistoryM>()
                    .ForPath(dest => dest.MovieNightGrade, opt => 
                        opt.MapFrom(src => src.Movie.MovieNightGrade))
                    .ForPath(dist => dist.YearOfRelease, opt =>
                        opt.MapFrom(src => src.Movie.ProductionYear));
            });


            MapperViewList = configV.CreateMapper();
            return new List<IMapper>
            {
                MapperCard,MapperFact,MapperCast,MapperFilm,MapperViewList
            };
        }


        protected MovieTemplateInfE GetMovieFromDb(int? id)
        {
            GetMappersSettings();
            var movieDb = new MovieTemplateInfE();
            var op = ReadMoviesFromJson("D:\\web project\\Movie\\MovieNight\\MovieNight.BusinessLogic\\DBModel\\Seed\\SeedData.json");

            PopulateDatabase(op);
            // conf.CreateMapper();
            // var maper = conf.CreateMapper();
            using (var db = new MovieContext())
            {
                try
                {
                    var movieS = db.MovieDb.FirstOrDefault(m => m.Id == id);
                    if (movieS != null)
                    {
                        movieDb = MapperFilm.Map<MovieTemplateInfE>(movieS);
                        var listOfCast = db.CastDbTables.Where(cast => cast.Movies.Any(movie => movie.Id == id)).ToList();
                        if (listOfCast != null)
                        {
                            movieDb.CastMembers = new List<CastMemberE>();
                            foreach (var cast in listOfCast)
                            {
                                var onCast = MapperCast.Map<CastMemberE>(cast);
                                movieDb.CastMembers.Add(onCast);
                            }
                        }
                        var listOfFacts = db.InterestingFact.Where(fact => fact.MovieId == id).ToList();
                        if (listOfFacts != null)
                        {
                            movieDb.InterestingFacts = new List<InterestingFactE>();
                            foreach (var cast in listOfFacts)
                            {
                                var onFact = MapperFact.Map<InterestingFactE>(cast);
                                movieDb.InterestingFacts.Add(onFact);
                            }
                            
                        }
                        var listOfMovieCards = db.MovieCard.Where(card => card.MovieId == id).ToList();
                        if (listOfMovieCards != null)
                        {
                            movieDb.MovieCards = new List<MovieCardE>();
                            foreach (var card in listOfMovieCards)
                            {
                                var onCard = MapperCard.Map<MovieCardE>(card);
                                movieDb.MovieCards.Add(onCard);
                            }
                        }

                        movieDb.Genre = new List<string>();
                        
                            movieDb.Genre = JsonConvert.DeserializeObject<List<string>>(movieS.Genres);
                        
                    }

                    return movieDb;
                }
                catch (NullReferenceException ex)
                {
                    Console.WriteLine(@"Ошибка: " + ex.Message);
                    Console.WriteLine(@"StackTrace: " + ex.StackTrace);
                    return null;
                }
                
            }

        }

        private void PopulateDatabase(List<MovieTemplateInfE> movies)
        {
            using (var db = new MovieContext())
            {
                
                foreach (var movieTemplate in movies)
                {
                    try
                    {
                        
                        var movieDb = new MovieDbTable
                        {
                            Title = movieTemplate.Title,
                            Category = movieTemplate.Category,
                            PosterImage = movieTemplate.PosterImage,
                            Quote = movieTemplate.Quote,
                            Description = movieTemplate.Description,
                            ProductionYear = movieTemplate.ProductionYear,
                            Country = movieTemplate.Country,
                            Genres = JsonConvert.SerializeObject(movieTemplate.Genre), 
                            Location = movieTemplate.Location,
                            Director = movieTemplate.Director,
                            Duration = DateTime.Parse(movieTemplate.DurationJ), 
                            MovieNightGrade = movieTemplate.MovieNightGrade,
                            Certificate = movieTemplate.Certificate,
                            ProductionCompany = movieTemplate.ProductionCompany,
                            Budget = movieTemplate.Budget,
                            GrossWorldwide = movieTemplate.GrossWorldwide,
                            Language = movieTemplate.Language,
                            CastMembers = new List<CastMemDbTable>(),
                            InterestingFacts = new List<InterestingFactDbTable>(),
                            MovieCards = new List<MovieCardDbTable>()
                        };
                        db.MovieDb.Add(movieDb);
                        //db.SaveChanges();

                        foreach (var factE in movieTemplate.InterestingFacts)
                        {
                            var fact = new InterestingFactDbTable
                            {
                                FactName = factE.FactName,
                                FactText = factE.FactText
                            };

                            movieDb.InterestingFacts.Add(fact);
                            db.InterestingFact.Add(fact);
                            //db.SaveChanges();
                        }

                        foreach (var cardE in movieTemplate.MovieCards)
                        {
                            var card = new MovieCardDbTable()
                            {
                                ImageUrl = cardE.ImageUrl,
                                Description = cardE.Description,
                                Title = cardE.Title
                            };

                            movieDb.MovieCards.Add(card);
                            db.MovieCard.Add(card);
                            //db.SaveChanges();
                        }

                        foreach (var member in movieTemplate.CastMembers)
                        {
                            // movieDb.CastMembers.Add(new CastMemDbTable
                            // {
                            //     Name = member.Name,
                            //     ImageUrl = member.ImageUrl,
                            //     Role = member.Role
                            // });

                            var castMemberDb = new CastMemDbTable
                            {
                                Name = member.Name,
                                ImageUrl = member.ImageUrl,
                                Role = member.Role
                            };
                            movieDb.CastMembers.Add(castMemberDb);
                            db.CastDbTables.Add(castMemberDb);
                            // db.SaveChanges();

                        }

                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException ex)
                    {
                        foreach (var validationErrors in ex.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                Console.WriteLine($@"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                            }
                        }
                    }
                }

            }
        }
        
        private List<MovieTemplateInfE> ReadMoviesFromJson(string url)
        {
            try
            {
                //string jsonFileName = "SeedData.json";
                //string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jsonFileName);
                string jsonFilePath = url;
                List<MovieTemplateInfE> moviess;

                string json = File.ReadAllText(jsonFilePath);
                Console.WriteLine(json);

                moviess = JsonConvert.DeserializeObject<List<MovieTemplateInfE>>(json);

                return moviess;

            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Console.WriteLine($@"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                    }
                }
                return null;
            }
        }

        protected async Task<BookmarkE> SetNewBookmarkDb((int user,int movie) idAdd)
        {
            var resp = new BookmarkE();
            try
            {
                using (var db = new UserContext())
                {
                    var verify = await db.Bookmark.FirstOrDefaultAsync(b => b.UserId == idAdd.user && b.MovieId == idAdd.movie);
            
                    if (verify == null )
                    {
                        var addBookmarkE = new BookmarkDbTable
                        {
                            UserId = idAdd.user,
                            MovieId = idAdd.movie,
                            TimeAdd = DateTime.Now,
                            BookmarkTimeOf = false,
                            BookMark = true
                        };
                        db.Bookmark.Add(addBookmarkE);
                        await db.SaveChangesAsync();
                        resp.Msg = "Success";
                        resp.Success = true;
                        return resp;
                    }
                    else
                    {
                        if (verify.BookmarkTimeOf)
                        {
                            verify.BookMark = true;
                            resp.Msg = "Have already been added!";
                            resp.Success = true;
                        }
                        else
                        {
                            resp.Msg = "Have already been added!";
                            resp.Success = false;
                        }
                
                        await db.SaveChangesAsync();
                
                        return resp;
                    }
                }
            }
            catch (Exception ex)
            {
                resp.Msg = "Error: " + ex.Message;
                resp.Success = false;
                return resp;
            }
        }


        protected async Task<bool> DeleteBookmarkDb((int user,int movie) idAdd)
        {
            try
            {
                using (var db = new UserContext())
                {
                    var bookmarkToDelete = await db.Bookmark.FirstOrDefaultAsync(b => b.UserId == idAdd.user && b.MovieId == idAdd.movie);

                    if (bookmarkToDelete != null)
                    {
                        if (bookmarkToDelete.BookmarkTimeOf)
                        {
                            bookmarkToDelete.BookMark = false;
                            await db.SaveChangesAsync();
                            return true;
                        }
                        db.Bookmark.Remove(bookmarkToDelete);

                        await db.SaveChangesAsync();
                    }
                    else
                    {
                        return false;
                    }
                    
                }
            }catch (Exception ex)
            {
                Console.WriteLine($@"Error deleting bookmark: {ex.Message}");
                return  false;
            }

            return true;
        }

        protected bool GetInfBookmarkDb((int? user,int movie) movieid)
        {
            try
            {
                using (var db = new UserContext())
                {
                    var verify =  db.Bookmark.FirstOrDefault(b => b.UserId == movieid.user && b.MovieId == movieid.movie);

                    return verify != null && verify.BookMark;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Error deleting bookmark: {ex.Message}");
                return  false ;
            }
        }
        protected bool GetInfBookmarkTimeOfDb((int user,int movie) movieid)
        {
            try
            {
                using (var db = new UserContext())
                {
                    var verify =  db.Bookmark.FirstOrDefault(b => b.UserId == movieid.user && b.MovieId == movieid.movie);

                    return verify != null && verify.BookmarkTimeOf;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Error deleting bookmark: {ex.Message}");
                return  false ;
            }
        }


        protected List<ListOfFilmsE> GetListPlainDb(int? userId)
        {
            var listBookmark = new List<ListOfFilmsE>();

            try
            {
                using (var db = new UserContext())
                {
                    var dbList = db.Bookmark
                        .Where(l => l.UserId == userId && !l.BookmarkTimeOf)
                        .OrderByDescending(l => l.TimeAdd) 
                        .Take(5) 
                        .ToList();
                    foreach (var bookmarkDbTable in dbList)
                    {
                        using (var movie = new MovieContext())
                        {
                            var movieS = movie.MovieDb.FirstOrDefault(m => m.Id == bookmarkDbTable.MovieId);
                            if (movieS != null)
                            {
                                listBookmark.Add(new ListOfFilmsE
                                {
                                    MovieId = movieS.Id,
                                    Name = movieS.Title,
                                    Date = bookmarkDbTable.TimeAdd,
                                    NumberOfViews = db.ViewList.Count(n => n.UserId == userId),
                                    Category = movieS.Category,
                                    Star = movieS.MovieNightGrade,
                                    Genre = JsonConvert.SerializeObject(movieS.Genres)
                                });
                            }
                            
                           
                        }
                       
                    }

                    return listBookmark;

                }
            }
            catch (Exception ex)
            {
                
                return listBookmark;
            }
        }

        protected BookmarkTimeOfE GetListBookmarksTimeOfDb(int? userId)
        {

            var listBookmark = new BookmarkTimeOfE();
            GetMappersSettings();
            try
            {
                using (var db = new UserContext())
                {
                    var dbList = db.Bookmark
                        .Where(l => l.UserId == userId && l.BookmarkTimeOf)
                        .OrderByDescending(l => l.TimeAdd) 
                        .ToList();
                    foreach (var bookmarkDbTable in dbList)
                    {
                        using (var movie = new MovieContext())
                        {
                            var movieS = movie.MovieDb.FirstOrDefault(m => m.Id == bookmarkDbTable.MovieId);
                            if (movieS != null)
                            {
                                listBookmark.MovieInTimeOfBookmark.Add(MapperFilm.Map<MovieTemplateInfE>(movieS));
                                listBookmark.Bookmark.Add(new BookmarkE
                                {
                                    BookMark = bookmarkDbTable.BookMark,
                                    BookmarkTimeOf = bookmarkDbTable.BookmarkTimeOf,
                                    IdMovie = bookmarkDbTable.MovieId,
                                    IdUser = bookmarkDbTable.UserId,
                                    TimeAdd = bookmarkDbTable.TimeAdd
                                });
                            }
                            
                        }
                       
                    }

                    return listBookmark;

                }
            }
            catch (Exception ex)
            {
                
                return listBookmark;
            }
        }


        protected float GetUserRatingDb((int user, int movie) valueTuple)
        {
            
            try
            {
                using (var db = new UserContext())
                {
                    var verify =  db.ViewList?.FirstOrDefault(b => b.UserId == valueTuple.user && b.MovieId == valueTuple.movie);

                    if (verify != null) return verify.UserValues;
                }
            }
            catch (Exception ex)
            {
                
                return 0 ;
            }

            return 0;
        }

        protected async Task<bool> SetReteMovieAndViewDb((int user, int movieId, int rating) valueTuple)
        {
            //TODO:Add response model

            try
            {
                using (var db = new UserContext())
                {
                    var verify = await db.ViewList.FirstOrDefaultAsync(b => b.UserId == valueTuple.user && b.MovieId == valueTuple.movieId);

                    var ms = new MasterContext();

                    var movieC = ms.Movies.FirstOrDefaultAsync(m => m.Id == valueTuple.movieId);
                    
                    if (verify == null && movieC.Result != null )
                    {
                        if (movieC.Result != null)
                        {
                            


                                var addBookmarkE = new ViewListDbTable()
                                {
                                    UserId = valueTuple.user,
                                    MovieId = valueTuple.movieId,
                                    ReviewDate = DateTime.Now,
                                    TimeSpent = movieC.Result.Duration,
                                    UserValues = valueTuple.rating,
                                    UserViewCount = db.ViewList.Count(),
                                    Title = movieC.Result.Title,
                                    Category = movieC.Result.Category
                                };


                                db.ViewList.Add(addBookmarkE);
                            
                        }

                        await db.SaveChangesAsync();
                        return true;
                    }
                    else
                    {
                        if (movieC.Result != null)
                        {
                             verify.UserValues = valueTuple.rating;
                             db.Entry(verify).State = EntityState.Modified;
                            await db.SaveChangesAsync();
                            
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }


        }

        #region ViewList

        /// <summary>
        /// View List
        /// </summary>
        /// 
        protected List<ViewingHistoryM> GetViewingListDb(int? userId)
        {
            
            var viewingList = new List<ViewingHistoryM>();

            try
            {
                using (var db = new UserContext())
                {
                    var dbList = db.ViewList
                        .Where(l => l.UserId == userId)
                        .OrderByDescending(l => l.Id) 
                        .Take(5) 
                        .ToList();

                    foreach (var viewList in dbList)
                    {
                        using (var movie = new MovieContext())
                        {
                            var movieS = movie.MovieDb.FirstOrDefault(m => m.Id == viewList.MovieId);
                            if (movieS != null)
                            {
                                viewingList.Add(new ViewingHistoryM
                                {
                                    Title = movieS.Title,
                                    Description = movieS.Description,
                                    Id = movieS.Id,
                                    Poster = movieS.PosterImage,
                                    ReviewDate = viewList.ReviewDate,
                                    TimeSpent = viewList.TimeSpent,
                                    UserComment = viewList.UserComment,
                                    UserValues = viewList.UserValues,
                                    UserViewCount = viewList.UserViewCount,
                                    YearOfRelease = movieS.ProductionYear,
                                    MovieNightGrade = movieS.MovieNightGrade,
                                    Category = movieS.Category,
                                    
                                });
                            }
                            
                           
                        }
                       
                    }

                    return viewingList;
                }
            }
            catch (Exception ex)
            {
                
                return viewingList;
            }
        }


        protected Task<IEnumerable<ViewingHistoryM>> GetNewViewListDb(ViewListSortCommandE commandE)
        {
            List<ViewingHistoryM> currStateViewList;
            GetMappersSettings();
            using (var db = new UserContext() )
            {
                List<ViewListDbTable> preliminaryResult;
                if(!string.IsNullOrEmpty(commandE.SearchParameter)){
                    if(commandE.Category != FilmCategory.Non){
                        preliminaryResult = db.ViewList
                            .Where(l => l.Category == commandE.Category)
                            .Where(u => u.Title.StartsWith(commandE.SearchParameter))
                            .Include(viewListDbTable => viewListDbTable.Movie)
                            .ToList();
                    }
                    else
                    {
                        preliminaryResult = db.ViewList.Where(u => u.Title.StartsWith(commandE.SearchParameter))
                            .Include(viewListDbTable => viewListDbTable.Movie)
                            .ToList();;
                    }
                    
                     
                }
                else
                {
                    if (commandE.Category != FilmCategory.Non)
                    {
                        preliminaryResult = db.ViewList.Where(l => l.Category == commandE.Category)
                            .Include(viewListDbTable => viewListDbTable.Movie).ToList();
                    }
                    else
                    {
                        preliminaryResult = db.ViewList.Include(viewListDbTable => viewListDbTable.Movie).ToList();

                    }
                }
                
                using (var movieD = new MovieContext())
                {
                    if (commandE.SortingDirection != SortDirection.Non)
                    {
                        switch (commandE.Field)
                        {
                            case SelectField.Title:
                                preliminaryResult = commandE.SortingDirection == SortDirection.Ascending
                                    ? preliminaryResult.OrderBy(r => r.Title).ToList()
                                    : preliminaryResult.OrderByDescending(r => r.Title).ToList();
                                break;
                            case SelectField.YearOfRelease:
                                preliminaryResult = commandE.SortingDirection == SortDirection.Ascending
                                    ? preliminaryResult.OrderBy(r => r.Movie.ProductionYear).ToList()
                                    : preliminaryResult.OrderByDescending(r => r.Movie.ProductionYear).ToList();
                                break;
                            case SelectField.ReviewDate:
                                preliminaryResult = commandE.SortingDirection == SortDirection.Ascending
                                    ? preliminaryResult.OrderBy(r => r.ReviewDate).ToList()
                                    : preliminaryResult.OrderByDescending(r => r.ReviewDate).ToList();
                                break;
                            case SelectField.MovieNight:
                                preliminaryResult = commandE.SortingDirection == SortDirection.Ascending
                                    ? preliminaryResult.OrderBy(r => r.Movie.MovieNightGrade).ToList()
                                    : preliminaryResult.OrderByDescending(r => r.Movie.MovieNightGrade).ToList();
                                break;
                            case SelectField.UserValues:
                                preliminaryResult = commandE.SortingDirection == SortDirection.Ascending
                                    ? preliminaryResult.OrderBy(r => r.UserValues).ToList()
                                    : preliminaryResult.OrderByDescending(r => r.UserValues).ToList();
                                break;
                            default:
                                preliminaryResult = commandE.SortingDirection == SortDirection.Ascending
                                    ? preliminaryResult.OrderBy(r => r.Title).ToList()
                                    : preliminaryResult.OrderByDescending(r => r.Title).ToList();
                                break;
                        }
                    }
                }
                currStateViewList = MapperViewList.Map<List<ViewingHistoryM>>(preliminaryResult);
            }
            
            

            return Task.FromResult<IEnumerable<ViewingHistoryM>>(currStateViewList);
        }
        
        #endregion


        protected List<MovieTemplateInfE> GetListMovieDb(MovieCommandS movieSCommand)
        {
            GetMappersSettings();
            try
            {
                using (var dbMovie = new MovieContext())
                {
                    IQueryable<MovieDbTable> query = dbMovie.MovieDb;

                    if (movieSCommand.Direction != Direction.ForYou)
                    {
                        if (movieSCommand.Category != FilmCategory.Non)
                        {
                            query = query.Where(m => m.Category == movieSCommand.Category);
                        }

                        switch (movieSCommand.SortPar)
                        {
                            case SortingOption.ReleaseDate:
                                query = query.OrderBy(m => m.ProductionYear);
                                break;
                            case SortingOption.Popularity:
                                query = query.OrderByDescending(m => m.ViewListEntries.Count())
                                    .ThenByDescending(m => m.MovieNightGrade);
                                break;
                            case SortingOption.Grade:
                                query = query.OrderByDescending(m => m.MovieNightGrade);
                                break;
                            case SortingOption.Views:
                                query = query.OrderByDescending(m => m.ViewListEntries.Count());
                                break;
                        }
                    }
                    else if(movieSCommand.UserId != null)
                    {
                        using (var user = new UserContext())
                        {
                            var bookmarkedMovieIds = user.Bookmark
                                .Where(u => u.UserId == movieSCommand.UserId && !u.BookmarkTimeOf)
                                .Select(u => u.MovieId)
                                .ToList();

                            var bookmarkedGenres = dbMovie.MovieDb
                                .Where(m => bookmarkedMovieIds.Contains(m.Id))
                                .Select(m => m.Genres)
                                .ToList();

                            var viewedGenres = dbMovie.MovieDb
                                .Where(m => m.ViewListEntries.Any(v => v.UserId == movieSCommand.UserId))
                                .Select(m => m.Genres)
                                .ToList();

                            var allGenres = bookmarkedGenres.Concat(viewedGenres)
                                .SelectMany(genres => JsonConvert.DeserializeObject<List<string>>(genres))
                                .ToList();

                            var topGenres = allGenres.GroupBy(genre => genre)
                                .Select(group => new { Genre = group.Key, Count = group.Count() })
                                .OrderByDescending(x => x.Count)
                                .Take(5)
                                .Select(x => x.Genre)
                                .ToList();

                            var similarMovies = dbMovie.MovieDb
                                .AsEnumerable()
                                .Where(m => {
                                    var movieGenres = JsonConvert.DeserializeObject<List<string>>(m.Genres);
                                    var commonGenresCount = movieGenres.Intersect(topGenres).Count();
                                    return commonGenresCount >= 3; 
                                })
                                .Except(dbMovie.MovieDb.Where(m => bookmarkedMovieIds.Contains(m.Id) || m.ViewListEntries.Any(v => v.UserId == movieSCommand.UserId)))
                                .ToList();



                             
                             
                            if (movieSCommand.UserId != null)
                            {
                                var movieList1 = MapperFilm.Map<List<MovieTemplateInfE>>(similarMovies);
                                foreach (var movie in movieList1)
                                {
                                    var bookmarkDbTable = user.Bookmark
                                        .FirstOrDefaultAsync(u => u.UserId == movieSCommand.UserId && u.MovieId == movie.Id)
                                        .Result;
                                    if (bookmarkDbTable != null)
                                    {
                                        movie.BookmarkTomeOf = bookmarkDbTable.BookmarkTimeOf;
                                        movie.Bookmark = bookmarkDbTable.BookMark;
                                    }

                                }

                                return movieList1;
                            }
                            
                        }
                    }
                    
                    var movieDb = query.ToList();
                    if (movieSCommand.SortingDirection == SortDirection.Descending)
                    {
                        movieDb.Reverse();
                    }
                    
                    
                    
                   
                    var movieList = MapperFilm.Map<List<MovieTemplateInfE>>(movieDb);
                    using (var  user = new UserContext())
                    {
                        if (movieSCommand.UserId != null)
                        {

                            foreach (var movie in movieList)
                            {
                                var bookmarkDbTable = user.Bookmark
                                    .FirstOrDefaultAsync(u => u.UserId == movieSCommand.UserId && u.MovieId == movie.Id)
                                    .Result;
                                if (bookmarkDbTable != null)
                                {
                                    movie.BookmarkTomeOf = bookmarkDbTable.BookmarkTimeOf;
                                    movie.Bookmark = bookmarkDbTable.BookMark;
                                }

                            }
                        }
                    }
                    return movieList;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return new List<MovieTemplateInfE>();
            }
        }

        protected async Task<RespToAddBookmarkTimeOf> SetNewBookmarkTimeOfDb((int Id, int movieId) valueTuple)
        {
            GetMappersSettings();
            var respAdd = new RespToAddBookmarkTimeOf();
            var resp = new BookmarkE();
            var movie = new MovieContext().MovieDb.FirstOrDefault(m => m.Id == valueTuple.movieId);
            try
            {
                using (var db = new UserContext())
                {
                    var verify = await db.Bookmark.FirstOrDefaultAsync(b => b.UserId == valueTuple.Id && b.MovieId == valueTuple.movieId);
            
                    if (verify == null )
                    {
                        var addBookmarkE = new BookmarkDbTable
                        {
                            UserId = resp.IdUser = valueTuple.Id,
                            MovieId = resp.IdMovie = valueTuple.movieId,
                            TimeAdd = resp.TimeAdd = DateTime.Now,
                            BookmarkTimeOf = resp.BookmarkTimeOf = true
                        };
                        db.Bookmark.Add(addBookmarkE);
                        await db.SaveChangesAsync();
                        respAdd.RespMsg = resp.Msg = "Success";
                        respAdd.IsSuccese =  resp.Success = true;
                        respAdd.Bookmark = resp;
                        respAdd.MovieInTimeOfBookmark = MapperFilm.Map<MovieTemplateInfE>(movie);
                        
                        
                        return respAdd;
                    }
                    else
                    {
                        if (verify.BookMark)
                        {
                            verify.BookmarkTimeOf = resp.BookmarkTimeOf = true;
                            verify.TimeAdd = resp.TimeAdd = DateTime.Now;
                            resp.IdUser = valueTuple.Id;
                            resp.IdMovie = valueTuple.movieId;
                            resp.BookMark = verify.BookMark;

                        }
                        else
                        {
                            resp.Msg = "Have already been added! Error!";
                            resp.Success = false;
                            respAdd.Bookmark = resp;
                            respAdd.MovieInTimeOfBookmark = MapperFilm.Map<MovieTemplateInfE>(movie);
                            return respAdd;
                        }
                        await db.SaveChangesAsync();
                        resp.Msg = "Have already been added!";
                        resp.Success = true;
                        respAdd.Bookmark = resp;
                        respAdd.MovieInTimeOfBookmark = MapperFilm.Map<MovieTemplateInfE>(movie);
                        return  respAdd;
                    }
                }
            }
            catch (Exception ex)
            {
                respAdd.RespMsg = resp.Msg = "Error: " + ex.Message;
                respAdd.IsSuccese = resp.Success = false;
                respAdd.Bookmark = resp;
                return  respAdd;
            }

        }
        protected async Task<bool> DeleteBookmarkTimeOfDb((int user,int movie) idAdd)
        {
            try
            {
                using (var db = new UserContext())
                {
                    var bookmarkToDelete = await db.Bookmark.FirstOrDefaultAsync(b => b.UserId == idAdd.user && b.MovieId == idAdd.movie);

                    if (bookmarkToDelete != null)
                    {
                        if (bookmarkToDelete.BookmarkTimeOf && bookmarkToDelete.BookMark)
                        {
                            bookmarkToDelete.BookmarkTimeOf = false;
                            await db.SaveChangesAsync();
                            return true;
                        }else if (bookmarkToDelete.BookmarkTimeOf)
                        {
                            db.Bookmark.Remove(bookmarkToDelete);
                            await db.SaveChangesAsync();
                        }
                    }
                    else
                    {
                        return false;
                    }
                    
                }
            }catch (Exception ex)
            {
                Console.WriteLine($@"Error deleting bookmark: {ex.Message}");
                return  false;
            }

            return true;
        }

        protected void BookmarkStatusCheckDb()
        {
            try
            {
                using (var db = new UserContext())
                {
                    var currentTime = DateTime.Now;
                    var timeThreshold = currentTime.AddHours(-24);
                    
                    var expiredBookmarks = db.Bookmark
                        .Where(b => b.BookmarkTimeOf && !b.BookMark && b.TimeAdd < timeThreshold)
                        .ToList();
                    
                    db.Bookmark.RemoveRange(expiredBookmarks);
                    db.SaveChanges();
                }
                
                
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        protected Task<MovieTemplateInfE> GetRandomFilmDb()
        {
            try
            {
                GetMappersSettings();
                using (var movie = new MovieContext())
                {
                    var totalMoviesCount = movie.MovieDb.Count();
                
                    Random random = new Random();
                    int randomIndex = random.Next(0, totalMoviesCount);
                
                    var randomMovie = movie.MovieDb.OrderBy(f => Guid.NewGuid()).Skip(randomIndex).FirstOrDefault();

                    var movieR = MapperFilm.Map<MovieTemplateInfE>(randomMovie);

                    using (var user = new UserContext())
                    {
                        var userId = HttpContext.Current.Session["UserId"] as int?;
                        var bookmarkDbTable = user.Bookmark
                            .FirstOrDefaultAsync(u => u.UserId == userId  && u.MovieId == movieR.Id)
                            .Result;
                        if (bookmarkDbTable != null)
                        {
                            movieR.BookmarkTomeOf = bookmarkDbTable.BookmarkTimeOf;
                            movieR.Bookmark = bookmarkDbTable.BookMark;
                        }
                    }
                    
                    
                    return Task.FromResult(movieR);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return Task.FromResult(new MovieTemplateInfE());
                
            }
            
            


        }

        
    }
}