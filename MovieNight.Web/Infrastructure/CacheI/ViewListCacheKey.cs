using MovieNight.Domain.enams;

namespace MovieNight.Web.Infrastructure.CacheI
{
    public class ViewListCacheKey
    {
        public string SearchParameter { get; set; }
        public FilmCategory Category { get; set; }
        public SelectField Field { get; set; }
        public SortDirection SortingDirection { get; set; }
        

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (SearchParameter?.GetHashCode() ?? 0);
                hash = hash * 23 + Category.GetHashCode();
                hash = hash * 23 + Field.GetHashCode();
                hash = hash * 23 + SortingDirection.GetHashCode();
                return hash;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            var other = (ViewListCacheKey)obj;
            return SearchParameter == other.SearchParameter
                   && Category == other.Category
                   && Field == other.Field
                   && SortingDirection == other.SortingDirection;
        }
    }
}