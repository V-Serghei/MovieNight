"use client";

import { useEffect, useState } from "react";
import type { UIMovie } from "@/lib/types/movie/movie";
import { getMovieRatingsBulk } from "@/lib/api/ratings";

type MoviesResult = {
    data: UIMovie[] | null;
    loading: boolean;
    error: string | null;
};

export function useMoviesWithRatings(apiPath: string): MoviesResult {
    const [data, setData] = useState<UIMovie[] | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;

        (async () => {
            try {
                setLoading(true);
                setError(null);

                const resp = await fetch(apiPath, { credentials: "include" });
                if (!resp.ok) {
                    const text = await resp.text();
                    throw new Error(text || `Failed to load movies (${resp.status})`);
                }

                const raw = (await resp.json()) as any[];

                if (cancelled) return;

                let movies: UIMovie[] = raw.map((m) => ({
                    id: m.id,
                    title: m.title,
                    year: m.productionYear,
                    duration: m.duration ?? "",
                    posterImage: m.posterImage ?? "",
                    rating: 0,
                    userRating: null,
                }));

                const ids = movies.map((m) => m.id);
                const summaries = await getMovieRatingsBulk(ids);

                const dict = new Map(summaries.map((s) => [s.movieId, s]));

                movies = movies.map((m) => {
                    const s = dict.get(m.id);
                    return {
                        ...m,
                        rating: s?.averageRating ?? 0,
                        userRating: s?.userRating ?? null,
                    };
                });

                if (!cancelled) setData(movies);
            } catch (err: any) {
                if (!cancelled) setError(err.message ?? "Error loading movies");
            } finally {
                if (!cancelled) setLoading(false);
            }
        })();

        return () => {
            cancelled = true;
        };
    }, [apiPath]);

    return { data, loading, error };
}
export function useMoviesByFilmsWithRatings() {
    return useMoviesWithRatings("/api/gw/movies/films");
}
export function useCartoonsWithRatings() {
    return useMoviesWithRatings("/api/gw/movies/cartoons");
}

export function useAnimeWithRatings() {
    return useMoviesWithRatings("/api/gw/movies/anime");
}
export function useSerialWithRatings() {
    return useMoviesWithRatings("/api/gw/movies/serial");
}
