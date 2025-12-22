// app/cinema/films/page.tsx
"use client";

import { TopBar } from "@/components/top-bar";
import { MovieGrid } from "@/components/movie-grid";
import { useMoviesByFilms } from "@/lib/useMovies";
import { useMoviesByFilmsWithRatings } from "@/lib/useMoviesWithRatings";

export default function FilmsPage() {
    const { data, loading, error } = useMoviesByFilmsWithRatings();

    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12">
                {loading && (
                    <div className="text-muted-foreground">
                        Loading films…
                    </div>
                )}
                {error && (
                    <div className="text-destructive">Error: {error}</div>
                )}
                {data && <MovieGrid title="Films" movies={data} />}
            </main>
        </div>
    );
}
