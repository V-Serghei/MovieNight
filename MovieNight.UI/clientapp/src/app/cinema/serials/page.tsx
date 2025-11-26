// app/cinema/Serials/page.tsx
"use client";

import { TopBar } from "@/components/top-bar";
import { MovieGrid } from "@/components/movie-grid";
import { useMoviesByFilms } from "@/lib/useMovies";
import { useSerialWithRatings } from "@/lib/useMoviesWithRatings";

export default function SerialsPage() {
    const { data, loading, error } = useSerialWithRatings();

    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12">
                {loading && (
                    <div className="text-muted-foreground">
                        Loading Serials…
                    </div>
                )}
                {error && (
                    <div className="text-destructive">Error: {error}</div>
                )}
                {data && <MovieGrid title="Serials" movies={data} />}
            </main>
        </div>
    );
}
