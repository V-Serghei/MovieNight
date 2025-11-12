// значения должны совпадать с C# enum MoviePlayer.Domain.Enums.MovieCategory
export enum MovieCategory {
    Film = 0,
    Series = 1,
    Cartoon = 2,
    Anime = 3,
}

// красивые подписи
export const movieCategoryLabel: Record<MovieCategory, string> = {
    [MovieCategory.Film]: "Film",
    [MovieCategory.Series]: "Series",
    [MovieCategory.Cartoon]: "Cartoon",
    [MovieCategory.Anime]: "Anime",
};
