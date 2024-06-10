using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using MovieNight.Web.Models.Movie;

namespace MovieNight.Web.Infrastructure.CacheI
{
    public class FilmCache
    {
        private readonly MemoryCache _cache;

        public FilmCache()
        {
            _cache = MemoryCache.Default;
        }

        public List<MovieTemplateInfModel> GetCachedMovies(int pageNumber)
        {
            return _cache.Get($"Page_{pageNumber}") as List<MovieTemplateInfModel>;
        }

        public void SetCachedMovies(int pageNumber, List<MovieTemplateInfModel> movies)
        {
            var cacheItemPolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(30) // Кэш будет действителен 30 минут
            };
            _cache.Set($"Page_{pageNumber}", movies, cacheItemPolicy);
        }

        public bool IsPageCached(int pageNumber)
        {
            return _cache.Contains($"Page_{pageNumber}");
        }
    }
}