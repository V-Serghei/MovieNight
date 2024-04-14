using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using System.Web.Caching;
using AutoMapper;
using MovieNight.BusinessLogic.DBModel;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.MovieM.EfDbEntities;
using Newtonsoft.Json;

namespace MovieNight.BusinessLogic.Core.ServiceApi
{
    public class MovieAPI
    {
        //var op = ReadMoviesFromJson("D:\\web project\\Movie\\MovieNight\\MovieNight.BusinessLogic\\DBModel\\Seed\\SeedData.json");

        private IMapper MapperFilm{ get; set; }
        private IMapper MapperFact{ get; set; }
        private IMapper MapperCast{ get; set; }
        private IMapper MapperCard{ get; set; }

        private List<IMapper> LMapper{ get; set; }


        private List<IMapper> GetMappersSettings()  
        {
            var conf = new MapperConfiguration(cnf =>
            {
                cnf.CreateMap<MovieDbTable, MovieTemplateInfE>()
                    .ForMember(dist =>
                        dist.InterestingFacts, src =>
                        src.Ignore())
                    .ForMember(dist =>
                        dist.CastMembers, src => src.Ignore())
                    .ForMember(dist =>
                        dist.MovieCards, src =>
                        src.Ignore());
                cnf.CreateMap<MovieTemplateInfE, MovieDbTable>()
                    .ForMember(dist =>
                        dist.InterestingFacts, src =>
                        src.Ignore())
                    .ForMember(dist =>
                        dist.CastMembers, src => src.Ignore())
                    .ForMember(dist =>
                        dist.MovieCards, src =>
                        src.Ignore());

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
            return (LMapper = new List<IMapper>
            {
                MapperCard,MapperFact,MapperCast,MapperFilm
            });
        }
        
        
        public MovieTemplateInfE GetMovieFromDb(int? id)
        {
            GetMappersSettings();
            var movieDb = new MovieTemplateInfE();
            var op = ReadMoviesFromJson("D:\\web project\\Movie\\MovieNight\\MovieNight.BusinessLogic\\DBModel\\Seed\\SeedData.json");

            PopulateDatabase(op);
            //conf.CreateMapper();
            //var maper = conf.CreateMapper();
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

                    }

                    return movieDb;
                }
                catch (Exception ex)
                {
                    return null;
                }
                
            }

        }
        public void PopulateDatabase(List<MovieTemplateInfE> movies)
        {
            using (var db = new MovieContext())
            {


                foreach (var movieTemplate in movies)
                {
                    try
                    {
                        // Создаем новый объект MovieDbTable
                        var movieDb = new MovieDbTable
                        {
                            Title = movieTemplate.Title,
                            PosterImage = movieTemplate.PosterImage,
                            Quote = movieTemplate.Quote,
                            Description = movieTemplate.Description,
                            ProductionYear = movieTemplate.ProductionYear,
                            Country = movieTemplate.Country,
                            Genres = JsonConvert.SerializeObject(movieTemplate.Genre), // Сохраняем жанры в формате JSON
                            Location = movieTemplate.Location,
                            Director = movieTemplate.Director,
                            Duration = DateTime.Parse(movieTemplate.DurationJ), // Преобразуем строку в TimeSpan
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
                                Console.WriteLine($"Property: {validationError.PropertyName} Error: {validationError.ErrorMessage}");
                            }
                        }
                    }
                }

            }
        }
    
        
        
        
        
        private List<MovieTemplateInfE> ReadMoviesFromJson(string url)
        {
            //string jsonFileName = "SeedData.json";
            //string jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, jsonFileName);
            string jsonFilePath = url;
            List<MovieTemplateInfE> moviess = new List<MovieTemplateInfE>();

            string json = File.ReadAllText(jsonFilePath);

            moviess = JsonConvert.DeserializeObject<List<MovieTemplateInfE>>(json);

            return moviess;
        }

        
    }
}