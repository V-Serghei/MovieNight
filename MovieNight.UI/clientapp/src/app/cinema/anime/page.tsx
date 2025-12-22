// app/cinema/Anime/page.tsx
"use client";

import { TopBar } from "@/components/top-bar";
import { MovieGrid } from "@/components/movie-grid";
import { useMoviesByFilms } from "@/lib/useMovies";
import { useAnimeWithRatings } from "@/lib/useMoviesWithRatings";

export default function AnimePage() {
    const { data, loading, error } = useAnimeWithRatings();

    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12">
                {loading && (
                    <div className="text-muted-foreground">
                        Loading Anime…
                    </div>
                )}
                {error && (
                    <div className="text-destructive">Error: {error}</div>
                )}
                {data && <MovieGrid title="Anime" movies={data} />}
            </main>
        </div>
    );
}
