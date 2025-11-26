// src/lib/api/rating-bookmarks-orchestrator.ts
import type { UIMovie } from "@/lib/types/movie/movie";
import {
    addWatched,
    deleteAllForMovie,
} from "@/lib/api/bookmarks";
import {
    setMovieRating,
    deleteMovieRating,
    type MovieRatingSummary,
} from "@/lib/api/ratings";

export async function rateMovieWithWatched(
    movie: UIMovie,
    score: number,
): Promise<MovieRatingSummary> {
    const summary = await setMovieRating(movie.id, score);

    await addWatched(movie);


    return summary;
}

export async function removeRatingAndCleanup(
    movie: UIMovie,
): Promise<MovieRatingSummary> {
    const summary = await deleteMovieRating(movie.id);

    await deleteAllForMovie(movie.id);

    return summary;
}
