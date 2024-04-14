using System;
using System.Linq;
using AutoMapper;
using MovieNight.BusinessLogic.DBModel;
using MovieNight.Domain.Entities.MovieM;
using MovieNight.Domain.Entities.MovieM.EfDbEntities;

namespace MovieNight.BusinessLogic.Core.ServiceApi
{
    public class MovieAPI
    {
        public MovieTemplateInfE GetMovieFromDb(int? id)
        {
            var movieDb = new MovieTemplateInfE();

            var conf = new MapperConfiguration(cnf =>
            {
                cnf.CreateMap<MovieTemplateInfE, MovieDbTable>()
                    .ForMember(dist =>
                            dist.InterestingFacts,
                        opt => opt.MapFrom(src =>
                            src.InterestingFacts))
                    .ForMember(dist =>
                            dist.CastMembers,
                        opt => opt.MapFrom(src =>
                            src.CastMembers))
                    .ForMember(dist =>
                            dist.MovieCards,
                        opt => opt.MapFrom(src =>
                            src.MovieCards));

            }).CreateMapper();
            //conf.CreateMapper();
            //var maper = conf.CreateMapper();
            using (var db = new MovieContext())
            {
                try
                {
                    var movieS = db.MovieDb.FirstOrDefault(m => m.Id == id);
                    if (movieS != null)
                    {
                        
                    }
                }
                catch (Exception ex)
                {
                    return null;
                }
               
                
            }

            return movieDb;
        }
    }
}