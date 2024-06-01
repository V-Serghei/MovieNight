using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using System.Web.UI;
using AutoMapper;
using MovieNight.BusinessLogic.DBModel;
using MovieNight.BusinessLogic.Migrations.Movie;
using MovieNight.BusinessLogic.Migrations.Session;
using MovieNight.BusinessLogic.Migrations.User;
using MovieNight.Domain.enams;
using MovieNight.Domain.Entities.DifferentE;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.MovieM.EfDbEntities;
using MovieNight.Domain.Entities.MovieM.ResultsOfTheOperation;
using MovieNight.Domain.Entities.MovieM.SearchParam;
using MovieNight.Domain.Entities.PersonalP.PersonalPDb;
using MovieNight.Domain.Entities.Review;
using MovieNight.Domain.Entities.Statistics;
using Newtonsoft.Json;
using EntityState = System.Data.Entity.EntityState;

namespace MovieNight.BusinessLogic.Core.ServiceApi
{
    public class MovieAPI
    {
        //var op = ReadMoviesFromJson("D:\\web project\\Movie\\MovieNight\\MovieNight.BusinessLogic\\DBModel\\Seed\\SeedData.json");
        readonly string _jsonPath = Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.FullName ?? string.Empty, @"MovieNight.BusinessLogic\DBModel\Seed\SeedData.json");
        string _basePath = Directory.GetParent(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty)?
            .Parent?.Parent?.FullName; 
        private IMapper MapperFilm { get; set; }
        private IMapper MapperFact { get; set; }
        private IMapper MapperCast { get; set; }
        private IMapper MapperCard { get; set; }

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
                MapperCard, MapperFact, MapperCast, MapperFilm, MapperViewList
            };
        }
        public static void Initialize()
        {
            var api = new MovieAPI();
            var movies = api.ReadMoviesFromJson(api._jsonPath);
            api.PopulateDatabase(movies);
        }

        protected MovieTemplateInfE GetMovieFromDb(int? id)
        {
            GetMappersSettings();
            var movieDb = new MovieTemplateInfE();
        
            
            // var op = ReadMoviesFromJson(
            //     "D:\\web project\\Movie\\MovieNight\\MovieNight.BusinessLogic\\DBModel\\Seed\\SeedData.json");
            //var op = ReadMoviesFromJson(_jsonPath);

            //PopulateDatabase(op);
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
                        var listOfCast = db.CastDbTables.Where(cast => cast.Movies.Any(movie => movie.Id == id))
                            .ToList();
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
        protected int? GetMovieFromDb(MovieTemplateInfE movieE)
        {
            GetMappersSettings();
            var movieDb = new MovieTemplateInfE();
        
            
            // var op = ReadMoviesFromJson(
            //     "D:\\web project\\Movie\\MovieNight\\MovieNight.BusinessLogic\\DBModel\\Seed\\SeedData.json");
            //var op = ReadMoviesFromJson(_jsonPath);

            //PopulateDatabase(op);
            // conf.CreateMapper();
            // var maper = conf.CreateMapper();
            using (var db = new MovieContext())
            {
                try
                {
                    var movieS = db.MovieDb.FirstOrDefault(m => m.Title == movieE.Title &&
                                                                m.ProductionYear == movieE.ProductionYear && 
                                                                m.Director == movieE.Director);
                    if (movieS != null)
                    {
                        movieDb = MapperFilm.Map<MovieTemplateInfE>(movieS);
                        var listOfCast = db.CastDbTables.Where(cast => cast.Movies.Any(movie => movie.Id == movieS.Id))
                            .ToList();
                        if (listOfCast != null)
                        {
                            movieDb.CastMembers = new List<CastMemberE>();
                            foreach (var cast in listOfCast)
                            {
                                var onCast = MapperCast.Map<CastMemberE>(cast);
                                movieDb.CastMembers.Add(onCast);
                            }
                        }

                        var listOfFacts = db.InterestingFact.Where(fact => fact.MovieId == movieS.Id).ToList();
                        if (listOfFacts != null)
                        {
                            movieDb.InterestingFacts = new List<InterestingFactE>();
                            foreach (var cast in listOfFacts)
                            {
                                var onFact = MapperFact.Map<InterestingFactE>(cast);
                                movieDb.InterestingFacts.Add(onFact);
                            }
                        }

                        var listOfMovieCards = db.MovieCard.Where(card => card.MovieId == movieS.Id).ToList();
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

                    return movieDb.Id;
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
                        
                        var existingMovie = db.MovieDb.FirstOrDefault(m =>
                            m.Title == movieTemplate.Title &&
                            m.ProductionYear == movieTemplate.ProductionYear &&
                            m.Director == movieTemplate.Director);

                        if (existingMovie != null)
                        {
                            
                            continue;
                        }

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

                        foreach (var factE in movieTemplate.InterestingFacts)
                        {
                            var fact = new InterestingFactDbTable
                            {
                                FactName = factE.FactName,
                                FactText = factE.FactText
                            };

                            movieDb.InterestingFacts.Add(fact);
                            db.InterestingFact.Add(fact);
                        }

                        foreach (var cardE in movieTemplate.MovieCards)
                        {
                            var card = new MovieCardDbTable
                            {
                                ImageUrl = cardE.ImageUrl,
                                Description = cardE.Description,
                                Title = cardE.Title
                            };

                            movieDb.MovieCards.Add(card);
                            db.MovieCard.Add(card);
                        }

                        foreach (var member in movieTemplate.CastMembers)
                        {
                            var castMemberDb = new CastMemDbTable
                            {
                                Name = member.Name,
                                ImageUrl = member.ImageUrl,
                                Role = member.Role
                            };
                            movieDb.CastMembers.Add(castMemberDb);
                            db.CastDbTables.Add(castMemberDb);
                        }

                        db.SaveChanges();
                    }
                    catch (DbEntityValidationException ex)
                    {
                        foreach (var validationErrors in ex.EntityValidationErrors)
                        {
                            foreach (var validationError in validationErrors.ValidationErrors)
                            {
                                Console.WriteLine(
                                    $@"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
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
                

                moviess = JsonConvert.DeserializeObject<List<MovieTemplateInfE>>(json);

                return moviess;
            }
            catch (DbEntityValidationException ex)
            {
                foreach (var validationErrors in ex.EntityValidationErrors)
                {
                    foreach (var validationError in validationErrors.ValidationErrors)
                    {
                        Console.WriteLine(
                            $@"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                    }
                }

                return null;
            }
        }

        protected async Task<BookmarkE> SetNewBookmarkDb((int user, int movie) idAdd)
        {
            var resp = new BookmarkE();
            try
            {
                using (var db = new UserContext())
                {
                    var verify =
                        await db.Bookmark.FirstOrDefaultAsync(b => b.UserId == idAdd.user && b.MovieId == idAdd.movie);

                    if (verify == null)
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


        protected async Task<bool> DeleteBookmarkDb((int user, int movie) idAdd)
        {
            try
            {
                using (var db = new UserContext())
                {
                    var bookmarkToDelete =
                        await db.Bookmark.FirstOrDefaultAsync(b => b.UserId == idAdd.user && b.MovieId == idAdd.movie);

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
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Error deleting bookmark: {ex.Message}");
                return false;
            }

            return true;
        }

        protected bool GetInfBookmarkDb((int? user, int movie) movieid)
        {
            try
            {
                using (var db = new UserContext())
                {
                    var verify =
                        db.Bookmark.FirstOrDefault(b => b.UserId == movieid.user && b.MovieId == movieid.movie);

                    return verify != null && verify.BookMark;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Error deleting bookmark: {ex.Message}");
                return false;
            }
        }

        protected bool GetInfBookmarkTimeOfDb((int user, int movie) movieid)
        {
            try
            {
                using (var db = new UserContext())
                {
                    var verify =
                        db.Bookmark.FirstOrDefault(b => b.UserId == movieid.user && b.MovieId == movieid.movie);

                    return verify != null && verify.BookmarkTimeOf;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Error deleting bookmark: {ex.Message}");
                return false;
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
                    var verify = db.ViewList?.FirstOrDefault(b =>
                        b.UserId == valueTuple.user && b.MovieId == valueTuple.movie);

                    if (verify != null) return verify.UserValues;
                }
            }
            catch (Exception ex)
            {
                return 0;
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
                    var verify = await db.ViewList.FirstOrDefaultAsync(b =>
                        b.UserId == valueTuple.user && b.MovieId == valueTuple.movieId);

                    var ms = new MasterContext();

                    var movieC = ms.Movies.FirstOrDefaultAsync(m => m.Id == valueTuple.movieId);

                    if (verify == null && movieC.Result != null)
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
                                UserViewCount = 1,
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
                        .Take(10)
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

        protected List<ViewingHistoryM> GetBookmarkTimeOfListDb(int? userId)
        {
            var getBookmarkListDb = new List<ViewingHistoryM>();

            try
            {
                using (var db = new UserContext())
                {
                    var dbList = db.Bookmark
                        .Where(l => l.UserId == userId && l.BookmarkTimeOf)
                        .OrderByDescending(l => l.TimeAdd)
                        .Take(10)
                        .ToList();


                    foreach (var bookmarkDbTable in dbList)
                    {
                        using (var movie = new MovieContext())
                        {
                            var movieS = movie.MovieDb.FirstOrDefault(m => m.Id == bookmarkDbTable.MovieId);
                            if (movieS != null)
                            {
                                getBookmarkListDb.Add(new ViewingHistoryM
                                {
                                    Title = movieS.Title,
                                    Description = movieS.Description,
                                    Id = movieS.Id,
                                    Poster = movieS.PosterImage,
                                    ReviewDate = bookmarkDbTable.TimeAdd,
                                    TimeSpent = movieS.Duration,
                                    YearOfRelease = movieS.ProductionYear,
                                    MovieNightGrade = movieS.MovieNightGrade,
                                    Category = movieS.Category,
                                });
                            }
                        }
                    }

                    return getBookmarkListDb;
                }
            }
            catch (Exception ex)
            {
                return getBookmarkListDb;
            }
        }


        protected Task<IEnumerable<ViewingHistoryM>> GetNewViewListDb(ViewListSortCommandE commandE)
        {
            List<ViewingHistoryM> currStateViewList;
            GetMappersSettings();
            using (var db = new UserContext())
            {
                List<ViewListDbTable> preliminaryResult;
                if (!string.IsNullOrEmpty(commandE.SearchParameter))
                {
                    if (commandE.Category != FilmCategory.Non)
                    {
                        preliminaryResult = db.ViewList
                            .Where(l => l.Category == commandE.Category  && l.UserId == commandE.userId )
                            .Where(u => u.Title.StartsWith(commandE.SearchParameter))
                            .Include(viewListDbTable => viewListDbTable.Movie)
                            .ToList();
                    }
                    else
                    {
                        preliminaryResult = db.ViewList.Where(u => u.Title.StartsWith(commandE.SearchParameter) && u.UserId == commandE.userId)
                            .Include(viewListDbTable => viewListDbTable.Movie)
                            .ToList();
                        ;
                    }
                }
                else
                {
                    if (commandE.Category != FilmCategory.Non)
                    {
                        preliminaryResult = db.ViewList.Where(l => l.Category == commandE.Category && l.UserId == commandE.userId)
                            .Include(viewListDbTable => viewListDbTable.Movie).ToList();
                    }
                    else
                    {
                        preliminaryResult = db.ViewList.Where( l => l.UserId == commandE.userId).Include(viewListDbTable => viewListDbTable.Movie).ToList();
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
                    else if (movieSCommand.UserId != null)
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
                                .Where(m =>
                                {
                                    var movieGenres = JsonConvert.DeserializeObject<List<string>>(m.Genres);
                                    var commonGenresCount = movieGenres.Intersect(topGenres).Count();
                                    return commonGenresCount >= 3;
                                })
                                .Except(dbMovie.MovieDb.Where(m =>
                                    bookmarkedMovieIds.Contains(m.Id) ||
                                    m.ViewListEntries.Any(v => v.UserId == movieSCommand.UserId)))
                                .ToList();


                            if (movieSCommand.UserId != null)
                            {
                                var movieList1 = MapperFilm.Map<List<MovieTemplateInfE>>(similarMovies);
                                foreach (var movie in movieList1)
                                {
                                    var bookmarkDbTable = user.Bookmark
                                        .FirstOrDefaultAsync(u =>
                                            u.UserId == movieSCommand.UserId && u.MovieId == movie.Id)
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
                    using (var user = new UserContext())
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
                    var verify = await db.Bookmark.FirstOrDefaultAsync(b =>
                        b.UserId == valueTuple.Id && b.MovieId == valueTuple.movieId);

                    if (verify == null)
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
                        respAdd.IsSuccese = resp.Success = true;
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
                        return respAdd;
                    }
                }
            }
            catch (Exception ex)
            {
                respAdd.RespMsg = resp.Msg = "Error: " + ex.Message;
                respAdd.IsSuccese = resp.Success = false;
                respAdd.Bookmark = resp;
                return respAdd;
            }
        }

        protected async Task<bool> DeleteBookmarkTimeOfDb((int user, int movie) idAdd)
        {
            try
            {
                using (var db = new UserContext())
                {
                    var bookmarkToDelete =
                        await db.Bookmark.FirstOrDefaultAsync(b => b.UserId == idAdd.user && b.MovieId == idAdd.movie);

                    if (bookmarkToDelete != null)
                    {
                        if (bookmarkToDelete.BookmarkTimeOf && bookmarkToDelete.BookMark)
                        {
                            bookmarkToDelete.BookmarkTimeOf = false;
                            await db.SaveChangesAsync();
                            return true;
                        }
                        else if (bookmarkToDelete.BookmarkTimeOf)
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
            }
            catch (Exception ex)
            {
                Console.WriteLine($@"Error deleting bookmark: {ex.Message}");
                return false;
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

                    
                    var expiredBookmarksAndInBookmark = db.Bookmark
                        .Where(b => b.BookmarkTimeOf && b.BookMark && b.TimeAdd < timeThreshold)
                        .ToList(); 

                    foreach (var bookmark in expiredBookmarksAndInBookmark)
                    {
                        bookmark.BookmarkTimeOf = false;
                    }

                     
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        protected void ClearBookmarksDb()
        {
            try
            {
                using (var db = new UserContext())
                {
                    var bookmarksToUpdate = db.Bookmark.Where(b => b.BookmarkTimeOf && b.BookMark).ToList();
                    foreach (var bookmark in bookmarksToUpdate)
                    {
                        bookmark.BookmarkTimeOf = false;
                    }

                    var bookmarksToDelete = db.Bookmark.Where(b => b.BookmarkTimeOf && !b.BookMark).ToList();
                    db.Bookmark.RemoveRange(bookmarksToDelete);

                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }


        protected async Task<MovieTemplateInfE> GetRandomFilmDb()
        {
            try
            {
                GetMappersSettings();
                using (var movie = new MovieContext())
                {
                    movie.Database.CommandTimeout = 120; 

                    
                    var randomMovies = await movie.MovieDb
                        .OrderBy(f => Guid.NewGuid())
                        .Take(1)
                        .FirstOrDefaultAsync();

                    if (randomMovies == null)
                    {
                        return null; 
                    }

                    var movieR = MapperFilm.Map<MovieTemplateInfE>(randomMovies);

                    using (var user = new UserContext())
                    {
                        var userId = HttpContext.Current.Session["UserId"] as int?;
                        var bookmarkDbTable = await user.Bookmark
                            .FirstOrDefaultAsync(u => u.UserId == userId && u.MovieId == movieR.Id);

                        if (bookmarkDbTable != null)
                        {
                            movieR.BookmarkTomeOf = bookmarkDbTable.BookmarkTimeOf;
                            movieR.Bookmark = bookmarkDbTable.BookMark;
                        }
                    }

                    return movieR;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new MovieTemplateInfE();
            }
        }

        protected async Task<InfMovieScoresE> GetInfOnFilmScoresDb(int? userId)
        {
            try
            {
                var listStatisticInf = new InfMovieScoresE();
                using (var db = new UserContext())
                {
                    using (var movie = new MovieContext())
                    {
                        var takeL = await db.ViewList.CountAsync();
                        if (takeL > 100) takeL = 100;
                        var viewList = db.ViewList.OrderBy(u => u.ReviewDate).Where(u => u.UserId == userId).Take(takeL)
                            .ToList();
                        if (viewList != null)
                        {
                            foreach (var viewListDbTable in viewList)
                            {
                                listStatisticInf.IdMovie.Add(viewListDbTable.Id);
                                var movieDbTable = await movie.MovieDb
                                    .FirstOrDefaultAsync(m => m.Id == viewListDbTable.MovieId);
                                if (movieDbTable !=
                                    null)
                                    listStatisticInf.MovieNightGrade.Add(movieDbTable
                                        .MovieNightGrade);
                                listStatisticInf.MyGrades.Add(viewListDbTable.UserValues);
                                listStatisticInf.TitleMovie.Add(viewListDbTable.Title);
                                listStatisticInf.DataAddGrade.Add(viewListDbTable.ReviewDate.ToString("MM/dd/yyyy"));
                            }

                            return listStatisticInf;
                        }

                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        protected StatisticE GetDataStatisticPageApi(int? userId)
        {
            try
            {
                var statisticData = new StatisticE();
                if (userId != null)
                {
                    using (var user = new UserContext())
                    {
                        using (var movie = new MovieContext())
                        {
                            var listView = user.ViewList.Where(us=> us.UserId == userId)
                                .OrderByDescending(v => v.ReviewDate)
                                .Include(viewListDbTable => viewListDbTable.Movie).ToList();
                            foreach (var listDbTable in listView)
                            {
                                statisticData.ViewList.Add(new ViewingHistoryM
                                {
                                    Id = listDbTable.MovieId,
                                    Title = listDbTable.Title,
                                    ReviewDate = listDbTable.ReviewDate,
                                    UserValues = listDbTable.UserValues,
                                    MovieNightGrade = listDbTable.Movie.MovieNightGrade
                                });
                            }

                            statisticData.AnimeCount = user.ViewList.Count(c =>
                                c.Category == FilmCategory.Anime && c.UserId == userId);
                            statisticData.AnimeTotal = movie.MovieDb.Count(c => c.Category == FilmCategory.Anime);
                            statisticData.CartonsCount = user.ViewList.Count(c =>
                                c.Category == FilmCategory.Cartoon && c.UserId == userId);
                            statisticData.CartonTotal = movie.MovieDb.Count(c => c.Category == FilmCategory.Cartoon);
                            statisticData.FilmCount = user.ViewList.Count(c =>
                                c.Category == FilmCategory.Film && c.UserId == userId);
                            statisticData.FilTotal = movie.MovieDb.Count(c => c.Category == FilmCategory.Film);
                            statisticData.SerialsCount = user.ViewList.Count(c =>
                                c.Category == FilmCategory.Serial && c.UserId == userId);
                            statisticData.SerialTotal = movie.MovieDb.Count(c => c.Category == FilmCategory.Serial);

                            statisticData.ViewingCount = user.ViewList.Count(c => c.UserId == userId);
                            statisticData.BookmarkCount = user.Bookmark.Count(c => c.UserId == userId);

                            var Genres = movie.MovieDb
                                .Select(m => m.Genres)
                                .ToList();

                            var allGenres = Genres
                                .SelectMany(genres => JsonConvert.DeserializeObject<List<string>>(genres))
                                .Distinct()
                                .ToList()
                                .Count();
                            statisticData.GenreTotal = allGenres;
                            Genres = movie.MovieDb
                                .Where(m => m.ViewListEntries.Any(v => v.UserId == userId))
                                .Select(m => m.Genres)
                                .ToList();

                            allGenres = Genres
                                .SelectMany(genres => JsonConvert.DeserializeObject<List<string>>(genres))
                                .Distinct()
                                .ToList()
                                .Count();
                            statisticData.YourGenrePrefer = allGenres;


                            var viewedLocations = movie.MovieDb
                                .Select(m => m.Location)
                                .ToList();

                            var allLocations = string.Join(",", viewedLocations).Split(',').Distinct().ToList();

                            statisticData.CountryTotal = allLocations.Count;

                            var userLocations = movie.MovieDb
                                .Where(m => m.ViewListEntries.Any(v => v.UserId == userId))
                                .Select(m => m.Location)
                                .ToList();

                            var userUniqueLocations = string.Join(",", userLocations).Split(',').Distinct().ToList();

                            statisticData.YourCountryPrefer = userUniqueLocations.Count;

                            statisticData.YourGradeCount = user.ViewList.Count(c => c.UserId == userId);
                            var mostCommonRating = listView
                                .Where(entry => entry.UserId == userId)
                                .GroupBy(entry => entry.UserValues)
                                .OrderByDescending(group => group.Count())
                                .Select(group => group.Key)
                                .FirstOrDefault();
                            statisticData.YourMostGrade = mostCommonRating;


                            var averageRating = listView
                                .Where(entry => entry.UserId == userId)
                                .Average(entry => entry.UserValues);
                            statisticData.YourAverageRating = averageRating;

                            return statisticData;
                        }
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        protected async Task<GenresDataStatistic> GetInfOnFilmGenresDb(int? userId)
        {
            try
            {
                var statisticData = new GenresDataStatistic();
                if (userId != null)
                {
                    using (var user = new UserContext())
                    {
                        var listView = user.ViewList.Where(u => u.UserId == userId).ToList();


                        var toListGenres = user.ViewList.Where(us => us.UserId == userId)
                            .Select(m => m.Movie.Genres)
                            .ToList();

                        var allGenres = toListGenres
                            .SelectMany(genres => JsonConvert.DeserializeObject<List<string>>(genres))
                            .ToList();

                        var topGenres = allGenres.GroupBy(genre => genre)
                            .Select(group => new { Genre = group.Key, Count = group.Count() })
                            .OrderByDescending(x => x.Count)
                            .ToList();
                        if (topGenres.Count > 20)
                        {
                            var topNineteenGenres = topGenres.Take(19).ToList();

                            int otherCount = topGenres.Skip(19).Sum(x => x.Count);

                            topNineteenGenres.Add(new { Genre = "Other", Count = otherCount });

                            topGenres = topNineteenGenres;
                        }


                        foreach (var genre in topGenres)
                        {
                            statisticData.GenresOrCountry.Add(genre.Genre);
                            statisticData.CountGenreOrCountry.Add(genre.Count);
                        }

                        return statisticData;
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        protected async Task<GenresDataStatistic> GetInfOnFilmCountryDb(int userId)
        {
            try
            {
                var statisticData = new GenresDataStatistic();
                if (userId != null)
                {
                    using (var user = new UserContext())
                    {
                        var toListCountry = user.ViewList.Where(us => us.UserId == userId)
                            .Select(m => m.Movie.Location)
                            .ToList();

                        var userUniqueLocations = string.Join(",", toListCountry).Split(',').ToList();


                        var countryList = userUniqueLocations.GroupBy(genre => genre)
                            .Select(group => new { Country = group.Key, Count = group.Count() })
                            .OrderByDescending(x => x.Count)
                            .ToList();
                        if (countryList.Count > 20)
                        {
                            var topNineteenGenres = countryList.Take(19).ToList();

                            int otherCount = countryList.Skip(19).Sum(x => x.Count);

                            topNineteenGenres.Add(new { Country = "Other", Count = otherCount });

                            countryList = topNineteenGenres;
                        }


                        foreach (var country in countryList)
                        {
                            statisticData.GenresOrCountry.Add(country.Country);
                            statisticData.CountGenreOrCountry.Add(country.Count);
                        }

                        return statisticData;
                    }
                }

                return null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }


        protected async Task<RespAddViewListElDb> SetViewListDb((int? movieId, int? Id) valueTuple)
        {
            try
            {
                if (valueTuple.movieId != null)
                {
                    if (valueTuple.Id != null)
                    {
                        using (var movieDb = new MovieContext())
                        {
                            using (var userDb = new UserContext())
                            {
                                var check = userDb.ViewList.Any(u =>
                                    u.UserId == valueTuple.Id && u.MovieId == valueTuple.movieId);
                                if (!check)
                                {
                                    var movieDbTable = movieDb.MovieDb
                                        .FirstOrDefaultAsync(m => m.Id == valueTuple.movieId)
                                        ?.Result;
                                    if (movieDbTable != null)
                                    {
                                        var addDbViewEl = new ViewListDbTable
                                        {
                                            Title = movieDbTable?.Title,
                                            ReviewDate = DateTime.Now,
                                            Category = movieDbTable.Category,
                                            MovieId = (int)valueTuple.movieId,
                                            TimeSpent = movieDbTable.Duration,
                                            UserId = (int)valueTuple.Id,
                                            UserValues = 0,
                                            UserViewCount = 1,
                                            UserComment = ""
                                        };
                                        userDb.ViewList.Add(addDbViewEl);
                                        await userDb.SaveChangesAsync();
                                        return new RespAddViewListElDb
                                        {
                                            MsgResp = "Succese!",
                                            IsSuccese = true
                                        };
                                    }
                                    else
                                    {
                                        return new RespAddViewListElDb
                                        {
                                            MsgResp = "Error: Movie dont exist!",
                                            IsSuccese = false
                                        };
                                    }
                                }
                                else
                                {
                                    var movieDbTable = movieDb.MovieDb
                                        .FirstOrDefaultAsync(m => m.Id == valueTuple.movieId)
                                        ?.Result;
                                    var viewDb = userDb.ViewList.FirstOrDefault(u =>
                                        u.UserId == valueTuple.Id && u.MovieId == valueTuple.movieId);
                                    if (viewDb != null)
                                    {
                                        viewDb.UserViewCount++;
                                        viewDb.TimeSpent = viewDb.TimeSpent + viewDb.TimeSpent.TimeOfDay;
                                        await userDb.SaveChangesAsync();

                                        return new RespAddViewListElDb
                                        {
                                            MsgResp = "Succese!",
                                            IsSuccese = true
                                        };
                                    }

                                    return new RespAddViewListElDb
                                    {
                                        MsgResp = "Error:DB",
                                        IsSuccese = false
                                    };

                                }
                            }
                        }
                    }
                    else
                    {
                        if (HttpContext.Current.Session["UserId"] != null)
                        {
                            var userIdS = HttpContext.Current.Session["UserId"] as int?;
                            using (var movieDb = new MovieContext())
                            {
                                using (var userDb = new UserContext())
                                {
                                    var check = userDb.ViewList.Any(u =>
                                        u.UserId == userIdS && u.MovieId == valueTuple.movieId);
                                    if (!check)
                                    {
                                        var movieDbTable = movieDb.MovieDb
                                            .FirstOrDefaultAsync(m => m.Id == valueTuple.movieId)
                                            ?.Result;
                                        if (movieDbTable != null)
                                        {
                                            if (userIdS != null)
                                            {
                                                var addDbViewEl = new ViewListDbTable
                                                {
                                                    Title = movieDbTable?.Title,
                                                    ReviewDate = DateTime.Now,
                                                    Category = movieDbTable.Category,
                                                    MovieId = (int)valueTuple.movieId,
                                                    TimeSpent = movieDbTable.Duration,
                                                    UserId = (int)userIdS,
                                                    UserValues = 0,
                                                    UserViewCount = 1,
                                                    UserComment = ""
                                                };
                                                userDb.ViewList.Add(addDbViewEl);
                                                await userDb.SaveChangesAsync();
                                            }

                                            await movieDb.SaveChangesAsync();
                                            return new RespAddViewListElDb
                                            {
                                                MsgResp = "Succese!",
                                                IsSuccese = true
                                            };
                                        }
                                        else
                                        {
                                            return new RespAddViewListElDb
                                            {
                                                MsgResp = "Error: Movie dont exist!",
                                                IsSuccese = false
                                            };
                                        }
                                    }
                                    else
                                    {
                                        var viewDb = userDb.ViewList.FirstOrDefault(u =>
                                            u.UserId == valueTuple.Id && u.MovieId == valueTuple.movieId);
                                        if (viewDb != null)
                                        {
                                            viewDb.UserViewCount++;
                                            viewDb.TimeSpent = viewDb.TimeSpent + viewDb.TimeSpent.TimeOfDay;
                                            await userDb.SaveChangesAsync();

                                        }

                                        return new RespAddViewListElDb
                                        {
                                            MsgResp = "Error:DB",
                                            IsSuccese = false
                                        };

                                    }
                                }
                            }
                        }
                        else
                        {
                            return new RespAddViewListElDb
                            {
                                MsgResp = "Error:Session does not exist",
                                IsSuccese = false
                            };
                        }
                    }
                }

                return new RespAddViewListElDb
                {
                    MsgResp = "Error:Data error, movie does not exist",
                    IsSuccese = false

                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return new RespAddViewListElDb { MsgResp = "Error:" + e, IsSuccese = false };
            }
        }

        protected async Task<List<BookmarkInfoE>> GetNewBookmarkTimeOfListDb(ListSortCommandE commandE)
        {
            List<BookmarkInfoE> currStateViewList;
            GetMappersSettings();

            using (var db = new UserContext())
            {
                IQueryable<BookmarkDbTable> query = db.Bookmark.Include(b => b.Movie);

                if (commandE.UserId.HasValue)
                {
                    query = query.Where(b => b.UserId == commandE.UserId.Value && b.BookmarkTimeOf);
                }

                if (!string.IsNullOrEmpty(commandE.SearchParameter))
                {
                    query = query.Where(u => u.Movie.Title.StartsWith(commandE.SearchParameter));
                }

                if (commandE.Category != FilmCategory.Non)
                {
                    query = query.Where(l => l.Movie.Category == commandE.Category);
                }

                switch (commandE.Field)
                {
                    case SelectField.Title:
                        query = commandE.SortingDirection == SortDirection.Ascending
                            ? query.OrderBy(r => r.Movie.Title)
                            : query.OrderByDescending(r => r.Movie.Title);
                        break;
                    case SelectField.YearOfRelease:
                        query = commandE.SortingDirection == SortDirection.Ascending
                            ? query.OrderBy(r => r.Movie.ProductionYear)
                            : query.OrderByDescending(r => r.Movie.ProductionYear);
                        break;
                    case SelectField.BookmarkDate:
                        query = commandE.SortingDirection == SortDirection.Ascending
                            ? query.OrderBy(r => r.TimeAdd)
                            : query.OrderByDescending(r => r.TimeAdd);
                        break;
                    case SelectField.OverallRating:
                        query = commandE.SortingDirection == SortDirection.Ascending
                            ? query.OrderBy(r => r.Movie.MovieNightGrade)
                            : query.OrderByDescending(r => r.Movie.MovieNightGrade);
                        break;
                    default:
                        query = commandE.SortingDirection == SortDirection.Ascending
                            ? query.OrderBy(r => r.Movie.Title)
                            : query.OrderByDescending(r => r.Movie.Title);
                        break;
                }

                var preliminaryResult = await query.ToListAsync();

                currStateViewList = preliminaryResult.Select(bookmark => new BookmarkInfoE
                {
                    Title = bookmark.Movie.Title,
                    MovieId = bookmark.Movie.Id,
                    BookmarkDate = bookmark.TimeAdd,
                    YearOfRelease = bookmark.Movie.ProductionYear,
                    OverallRating = bookmark.Movie.MovieNightGrade,
                    Category = bookmark.Movie.Category
                }).ToList();
            }

            return currStateViewList;
        }


        protected  List<BookmarkInfoE> GetListBookmarksTimeOfInfoDb(int? id)
        {
            var listBookmark = new List<BookmarkInfoE>();
            GetMappersSettings();
            try
            {
                using (var db = new UserContext())
                {
                    var dbList = db.Bookmark
                        .Where(l => l.UserId == id && l.BookmarkTimeOf)
                        .OrderByDescending(l => l.TimeAdd)
                        .ToList();
                    foreach (var bookmarkDbTable in dbList)
                    {
                        using (var movie = new MovieContext())
                        {
                            var movieS = movie.MovieDb.FirstOrDefault(m => m.Id == bookmarkDbTable.MovieId);
                            if (movieS != null)
                            {
                                listBookmark.Add(new BookmarkInfoE
                                {
                                    Title = movieS.Title,
                                    MovieId = movieS.Id,
                                    BookmarkDate = bookmarkDbTable.TimeAdd,
                                    Category = movieS.Category,
                                    OverallRating = movieS.MovieNightGrade,
                                    YearOfRelease = movieS.ProductionYear
                                });

                            }
                        }
                    }

                    return listBookmark;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            
            
            
        }
        
        
          protected async Task<List<BookmarkInfoE>> GetNewBookmarkListDb(ListSortCommandE commandE)
        {
            List<BookmarkInfoE> currStateViewList;
            GetMappersSettings();

            using (var db = new UserContext())
            {
                IQueryable<BookmarkDbTable> query = db.Bookmark.Include(b => b.Movie);

                if (commandE.UserId.HasValue)
                {
                    query = query.Where(b => b.UserId == commandE.UserId.Value && b.BookMark);
                }

                if (!string.IsNullOrEmpty(commandE.SearchParameter))
                {
                    query = query.Where(u => u.Movie.Title.StartsWith(commandE.SearchParameter));
                }

                if (commandE.Category != FilmCategory.Non)
                {
                    query = query.Where(l => l.Movie.Category == commandE.Category);
                }

                switch (commandE.Field)
                {
                    case SelectField.Title:
                        query = commandE.SortingDirection == SortDirection.Ascending
                            ? query.OrderBy(r => r.Movie.Title)
                            : query.OrderByDescending(r => r.Movie.Title);
                        break;
                    case SelectField.YearOfRelease:
                        query = commandE.SortingDirection == SortDirection.Ascending
                            ? query.OrderBy(r => r.Movie.ProductionYear)
                            : query.OrderByDescending(r => r.Movie.ProductionYear);
                        break;
                    case SelectField.BookmarkDate:
                        query = commandE.SortingDirection == SortDirection.Ascending
                            ? query.OrderBy(r => r.TimeAdd)
                            : query.OrderByDescending(r => r.TimeAdd);
                        break;
                    case SelectField.OverallRating:
                        query = commandE.SortingDirection == SortDirection.Ascending
                            ? query.OrderBy(r => r.Movie.MovieNightGrade)
                            : query.OrderByDescending(r => r.Movie.MovieNightGrade);
                        break;
                    default:
                        query = commandE.SortingDirection == SortDirection.Ascending
                            ? query.OrderBy(r => r.Movie.Title)
                            : query.OrderByDescending(r => r.Movie.Title);
                        break;
                }

                var preliminaryResult = await query.ToListAsync();

                currStateViewList = preliminaryResult.Select(bookmark => new BookmarkInfoE
                {
                    Title = bookmark.Movie.Title,
                    MovieId = bookmark.Movie.Id,
                    BookmarkDate = bookmark.TimeAdd,
                    YearOfRelease = bookmark.Movie.ProductionYear,
                    OverallRating = bookmark.Movie.MovieNightGrade,
                    Category = bookmark.Movie.Category
                }).ToList();
            }

            return currStateViewList;
        }


        protected  List<BookmarkInfoE> GetListBookmarksInfoDb(int? id)
        {
            var listBookmark = new List<BookmarkInfoE>();
            GetMappersSettings();
            try
            {
                using (var db = new UserContext())
                {
                    var dbList = db.Bookmark
                        .Where(l => l.UserId == id && l.BookMark)
                        .OrderByDescending(l => l.TimeAdd)
                        .ToList();
                    foreach (var bookmarkDbTable in dbList)
                    {
                        using (var movie = new MovieContext())
                        {
                            var movieS = movie.MovieDb.FirstOrDefault(m => m.Id == bookmarkDbTable.MovieId);
                            if (movieS != null)
                            {
                                listBookmark.Add(new BookmarkInfoE
                                {
                                    Title = movieS.Title,
                                    MovieId = movieS.Id,
                                    BookmarkDate = bookmarkDbTable.TimeAdd,
                                    Category = movieS.Category,
                                    OverallRating = movieS.MovieNightGrade,
                                    YearOfRelease = movieS.ProductionYear
                                });

                            }
                        }
                    }

                    return listBookmark;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            
            
            
        }

        protected async Task<List<MovieTemplateInfE>> GetMoviesDb(string searchTerm)
        {
            try
            {
                GetMappersSettings();
                using (var movie = new MovieContext())
                {
                    var movieS = await movie.MovieDb
                        .Where(m => m.Title.Contains(searchTerm)).Include(l=>l.ViewListEntries)
                        .OrderByDescending(m => m.ViewListEntries.Count) 
                        .ThenByDescending(m => m.MovieNightGrade) 
                        .Take(20) 
                        .ToListAsync();

                    var listMovie = MapperFilm.Map<List<MovieTemplateInfE>>(movieS);
                    
                    return listMovie;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;

            }

        }
        
        

        public List<ReviewE> getListOfReviewsDb(int? filmId)
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<ReviewDbTable, ReviewE>()
                    .ForMember(g=>g.Film, 
                        d => d.Ignore())
                    .ForMember(a=>a.User, 
                        b => b.Ignore());
            });
            
            var mapper = config.CreateMapper();
            using (var db = new MovieContext())
            {
                try
                {
                    var existsInDb = db.Review.Where(m => m.FilmId == filmId).ToList();
                    var reviewList = mapper.Map<List<ReviewE>>(existsInDb);
                    using(var dbU = new UserContext())
                    {
                        foreach (var review in reviewList)
                        {
                            review.Film = db.MovieDb.FirstOrDefault(v => v.Id == review.FilmId)?.Title;
                            review.User = dbU.UsersT.FirstOrDefault(n => n.Id == review.UserId)?.UserName;
                        }
                        return reviewList;
                    }
                }
                catch (Exception exception)
                {
                    return null;
                }
            }
        }

        public bool SetReviewDb(ReviewE reviewE)
        {
            var config = new MapperConfiguration(c =>
            {
                c.CreateMap<ReviewE , ReviewDbTable>()
                    .ForMember(g=>g.Film, 
                        d => d.Ignore())
                    .ForMember(a=>a.User, 
                        b => b.Ignore());
            });
            
            var mapper = config.CreateMapper();
            using (var db = new MovieContext())
            {
                try
                {
                    var reviewTable = mapper.Map<ReviewDbTable>(reviewE);
                    db.Review.Add(reviewTable);
                    db.SaveChanges();
                    return true;
                }
                catch (Exception exception)
                {
                    return false;
                }
            }
        }
        
        public int? DeleteReviewDb(int? reviewE)
        {
            using (var db = new MovieContext())
            {
                try
                {
                    var reviewTable = db.Review.FirstOrDefault(l=>l.Id==reviewE);
                    if (reviewTable != null)
                    {
                        var idFilm = reviewTable.FilmId;
                        db.Review.Remove(reviewTable);
                        db.SaveChanges();
                        return idFilm;
                    }
                }
                catch (Exception exception)
                {
                    return null;
                }

                return null;
            }
        }

        protected List<AreWatchingE> GetMoviesAreWatchingDb(int? userId)
        {
            try
            {
                using (var mDb = new MovieContext())
                using (var uDb = new UserContext())
                {
                    var lastMonth = DateTime.Now.AddMonths(-1);

                    var viewListEntries = uDb.ViewList
                        .Where(v => v.ReviewDate >= lastMonth)
                        .ToList();

                    var movieCounts = viewListEntries
                        .GroupBy(v => v.MovieId)
                        .Select(g => new
                        {
                            MovieId = g.Key,
                            Count = g.Count()
                        })
                        .OrderByDescending(mc => mc.Count)
                        .ToList();

                    var movieIds = movieCounts.Select(mc => mc.MovieId).ToList();
                    var movies = mDb.MovieDb
                        .Where(m => movieIds.Contains(m.Id))
                        .ToList();

                    var bookmarks = userId.HasValue
                        ? uDb.Bookmark
                            .Where(b => b.UserId == userId && movieIds.Contains(b.MovieId))
                            .ToList()
                        : null;

                    var result = movieCounts.Select(mc => new AreWatchingE
                    {
                        Id = mc.MovieId,
                        Title = movies.First(m => m.Id == mc.MovieId).Title,
                        PosterImage = movies.First(m => m.Id == mc.MovieId).PosterImage,
                        ProductionYear = movies.First(m => m.Id == mc.MovieId).ProductionYear,
                        Rating = movies.First(m => m.Id == mc.MovieId).MovieNightGrade,
                        Genre = JsonConvert.DeserializeObject<List<string>>(
                            movies.First(m => m.Id == mc.MovieId).Genres),
                        CountWatching = mc.Count,
                        Bookmark = bookmarks != null && userId.HasValue && bookmarks.Any(b => b.MovieId == mc.MovieId && b.BookMark),
                        BookmarkTomeOf = bookmarks != null &&
                                         userId.HasValue &&
                                         bookmarks.Any(b => b.MovieId == mc.MovieId && b.BookmarkTimeOf)
                    }).Take(30).ToList();

                    return result;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        protected MovieAddResult AddMovieTemplateDb(MovieTemplateInfE movieTemplateInfE)
        {
            var result = new MovieAddResult();
            using (var db = new MovieContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        
                        if (string.IsNullOrWhiteSpace(movieTemplateInfE.Title) ||
                            string.IsNullOrWhiteSpace(movieTemplateInfE.Description) ||
                            movieTemplateInfE.ProductionYear == default(DateTime))
                        {
                            result.Result = false;
                            result.Message = "Required fields are not filled in.";
                            return result;
                        }
                        
                        var movieDb = new MovieDbTable
                        {
                            Title = movieTemplateInfE.Title,
                            Category = movieTemplateInfE.Category,
                            PosterImage = movieTemplateInfE.PosterImage,
                            Quote = movieTemplateInfE.Quote,
                            Description = movieTemplateInfE.Description,
                            ProductionYear = movieTemplateInfE.ProductionYear,
                            Country = movieTemplateInfE.Country,
                            Genres = JsonConvert.SerializeObject(movieTemplateInfE.Genre),
                            Location = movieTemplateInfE.Location,
                            Director = movieTemplateInfE.Director,
                            Duration = movieTemplateInfE.Duration,
                            MovieNightGrade = movieTemplateInfE.MovieNightGrade,
                            Certificate = movieTemplateInfE.Certificate,
                            ProductionCompany = movieTemplateInfE.ProductionCompany,
                            Budget = movieTemplateInfE.Budget,
                            GrossWorldwide = movieTemplateInfE.GrossWorldwide,
                            Language = movieTemplateInfE.Language
                        };

                        db.MovieDb.Add(movieDb);
                        db.SaveChanges();
                        movieDb.CastMembers = new List<CastMemDbTable>();

                        foreach (var member in movieTemplateInfE.CastMembers)
                        {
                            var castMemberDb = new CastMemDbTable
                            {
                                Name = member.Name,
                                ImageUrl = member.ImageUrl,
                                Role = member.Role,
                                
                            };
                            movieDb.CastMembers.Add(castMemberDb);
                            db.CastDbTables.Add(castMemberDb);
                        }
                        movieDb.MovieCards = new List<MovieCardDbTable>();

                        foreach (var card in movieTemplateInfE.MovieCards)
                        {
                            var movieCardDb = new MovieCardDbTable
                            {
                                Title = card.Title,
                                ImageUrl = card.ImageUrl,
                                Description = card.Description
                            };
                            movieDb.MovieCards.Add(movieCardDb);
                            db.MovieCard.Add(movieCardDb);
                        }
                        movieDb.InterestingFacts = new List<InterestingFactDbTable>();

                        foreach (var fact in movieTemplateInfE.InterestingFacts)
                        {
                            var interestingFactDb = new InterestingFactDbTable
                            {
                                FactName = fact.FactName,
                                FactText = fact.FactText
                            };
                            movieDb.InterestingFacts.Add(interestingFactDb);
                            db.InterestingFact.Add(interestingFactDb);
                        }

                        db.SaveChanges();
                        transaction.Commit();

                        result.Result = true;
                        result.Movie = new MovieTemplateInfE();
                        result.Movie.Title = movieDb.Title;
                        result.Message = "Film successfully added.";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        result.Result = false;
                        result.Message = "Error adding film: " + ex.Message;
                    }
                }
            }

            return result;
        }

        protected MovieAddResult UpdateMovieTemplateDb(MovieTemplateInfE movieTemplateInfE)
        {
            GetMappersSettings();
            var result = new MovieAddResult();
            using (var db = new MovieContext())
            {
                using (var transaction = db.Database.BeginTransaction())
                {
                    try
                    {
                        var movieDb = db.MovieDb.Include(m => m.CastMembers)
                            .Include(m => m.MovieCards)
                            .Include(m => m.InterestingFacts)
                            .FirstOrDefault(m => m.Id == movieTemplateInfE.Id);

                        if (movieDb == null)
                        {
                            result.Result = false;
                            result.Message = "Movie not found.";
                            return result;
                        }

                        if (string.IsNullOrWhiteSpace(movieTemplateInfE.Title) ||
                            string.IsNullOrWhiteSpace(movieTemplateInfE.Description) ||
                            movieTemplateInfE.ProductionYear == default(DateTime) ||
                            string.IsNullOrWhiteSpace(movieTemplateInfE.Country))
                        {
                            result.Result = false;
                            result.Message = "Required fields are not filled in.";
                            return result;
                        }

                        movieDb.Title = movieTemplateInfE.Title;
                        movieDb.Category = movieTemplateInfE.Category;
                        movieDb.PosterImage = string.IsNullOrWhiteSpace(movieTemplateInfE.PosterImage)
                            ? movieDb.PosterImage
                            : movieTemplateInfE.PosterImage;
                        movieDb.Quote = movieTemplateInfE.Quote;
                        movieDb.Description = movieTemplateInfE.Description;
                        movieDb.ProductionYear = movieTemplateInfE.ProductionYear;
                        movieDb.Country = movieTemplateInfE.Country;
                        movieDb.Genres = JsonConvert.SerializeObject(movieTemplateInfE.Genre);
                        movieDb.Location = movieTemplateInfE.Location;
                        movieDb.Director = movieTemplateInfE.Director;
                        movieDb.Duration = movieTemplateInfE.Duration;
                        movieDb.MovieNightGrade = movieTemplateInfE.MovieNightGrade;
                        movieDb.Certificate = movieTemplateInfE.Certificate;
                        movieDb.ProductionCompany = movieTemplateInfE.ProductionCompany;
                        movieDb.Budget = movieTemplateInfE.Budget;
                        movieDb.GrossWorldwide = movieTemplateInfE.GrossWorldwide;
                        movieDb.Language = movieTemplateInfE.Language;

                        
                        var existingCastMembers = movieDb.CastMembers.ToList();
                        db.CastDbTables.RemoveRange(existingCastMembers);
                        movieDb.CastMembers.Clear();

                        foreach (var member in movieTemplateInfE.CastMembers)
                        {
                            var castMemberDb = new CastMemDbTable
                            {
                                Name = member.Name,
                                ImageUrl = string.IsNullOrWhiteSpace(member.ImageUrl)
                                    ? existingCastMembers.FirstOrDefault(m => m.Name == member.Name)?.ImageUrl
                                    : member.ImageUrl,
                                Role = member.Role
                            };
                            movieDb.CastMembers.Add(castMemberDb);
                            db.CastDbTables.Add(castMemberDb);
                        }

                        var existingMovieCards = movieDb.MovieCards.ToList();
                        db.MovieCard.RemoveRange(existingMovieCards);
                        movieDb.MovieCards.Clear();

                        foreach (var card in movieTemplateInfE.MovieCards)
                        {
                            var movieCardDb = new MovieCardDbTable
                            {
                                Title = card.Title,
                                ImageUrl = string.IsNullOrWhiteSpace(card.ImageUrl)
                                    ? existingMovieCards.FirstOrDefault(c => c.Title == card.Title)?.ImageUrl
                                    : card.ImageUrl,
                                Description = card.Description
                            };
                            movieDb.MovieCards.Add(movieCardDb);
                            db.MovieCard.Add(movieCardDb);
                        }

                        
                        db.InterestingFact.RemoveRange(movieDb.InterestingFacts);
                        movieDb.InterestingFacts.Clear();

                        foreach (var fact in movieTemplateInfE.InterestingFacts)
                        {
                            var interestingFactDb = new InterestingFactDbTable
                            {
                                FactName = fact.FactName,
                                FactText = fact.FactText
                            };
                            movieDb.InterestingFacts.Add(interestingFactDb);
                            db.InterestingFact.Add(interestingFactDb);
                        }

                        db.SaveChanges();
                        transaction.Commit();

                        result.Result = true;
                        result.Movie = MapperFilm.Map<MovieTemplateInfE>(movieDb);
                        result.Message = "Film successfully updated.";
                    }
                    catch (DbEntityValidationException ex)
                    {
                        transaction.Rollback();

                        foreach (var entityValidationError in ex.EntityValidationErrors)
                        {
                            Console.WriteLine(
                                $"Entity of type \"{entityValidationError.Entry.Entity.GetType().Name}\" in state \"{entityValidationError.Entry.State}\" has the following validation errors:");
                            foreach (var validationError in entityValidationError.ValidationErrors)
                            {
                                Console.WriteLine(
                                    $"- Property: \"{validationError.PropertyName}\", Error: \"{validationError.ErrorMessage}\"");
                            }
                        }

                        result.Result = false;
                        result.Message = "Validation Error: " + string.Join("; ",
                            ex.EntityValidationErrors.SelectMany(e => e.ValidationErrors).Select(e => e.ErrorMessage));
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        result.Result = false;
                        result.Message = "Error updating film: " + ex.Message;
                    }
                }
            }

            return result;
        }

        protected MovieDeleteResult DeleteMovieDb(int? id)
        {
            var result = new MovieDeleteResult();
            using (var dbMovie = new MovieContext())
            using (var dbUser = new UserContext())
            {
                dbMovie.Database.CommandTimeout = 300; 
                dbUser.Database.CommandTimeout = 300;

                using (var transaction = dbMovie.Database.BeginTransaction())
                {
                    try
                    {
                        var movieDb = dbMovie.MovieDb.Include(m => m.CastMembers)
                            .Include(m => m.MovieCards)
                            .Include(m => m.InterestingFacts)
                            .FirstOrDefault(m => m.Id == id);

                        if (movieDb == null)
                        {
                            result.Result = false;
                            result.Message = "Movie not found.";
                            return result;
                        }

                        
                        var viewListEntries = dbUser.ViewList.Where(v => v.MovieId == id).ToList();
                        dbUser.ViewList.RemoveRange(viewListEntries);
                        dbUser.SaveChanges(); 

                        
                        var bookmarks = dbUser.Bookmark.Where(b => b.MovieId == id).ToList();
                        dbUser.Bookmark.RemoveRange(bookmarks);
                        dbUser.SaveChanges(); 

                        
                        var reviews = dbMovie.Review.Where(r => r.FilmId == id).ToList();
                        dbMovie.Review.RemoveRange(reviews);
                        dbMovie.SaveChanges(); 

                        
                        dbMovie.CastDbTables.RemoveRange(movieDb.CastMembers);
                        dbMovie.MovieCard.RemoveRange(movieDb.MovieCards);
                        dbMovie.InterestingFact.RemoveRange(movieDb.InterestingFacts);

                        
                        dbMovie.MovieDb.Remove(movieDb);

                        dbMovie.SaveChanges(); 

                        transaction.Commit();

                        result.Result = true;
                        result.Message = "Film successfully deleted.";
                    }
                    catch (DbEntityValidationException ex)
                    {
                        transaction.Rollback();

                        foreach (var entityValidationError in ex.EntityValidationErrors)
                        {
                            Console.WriteLine(
                                $"Entity of type \"{entityValidationError.Entry.Entity.GetType().Name}\" in state \"{entityValidationError.Entry.State}\" has the following validation errors:");
                            foreach (var validationError in entityValidationError.ValidationErrors)
                            {
                                Console.WriteLine(
                                    $"- Property: \"{validationError.PropertyName}\", Error: \"{validationError.ErrorMessage}\"");
                            }
                        }

                        result.Result = false;
                        result.Message = "Validation Error: " + string.Join("; ",
                            ex.EntityValidationErrors.SelectMany(e => e.ValidationErrors).Select(e => e.ErrorMessage));
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        result.Result = false;
                        result.Message = "Error deleting film: " + ex.Message;
                    }
                }
            }

            return result;
        }



    }
}