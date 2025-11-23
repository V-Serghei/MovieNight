"use client";

import { useEffect, useState } from "react";
import { UIMovie } from "@/lib/types/movie/movie";

type MoviesResult = {
    data: UIMovie[] | null;
    loading: boolean;
    error: string | null;
};

export function useMoviesByFilms(): MoviesResult {
    const [data, setData] = useState<UIMovie[] | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;

        async function load() {
            try {
                setLoading(true);
                setError(null);

                const resp = await fetch("/api/gw/movies/films", {
                    credentials: "include",
                });

                if (!resp.ok) {
                    const text = await resp.text();
                    throw new Error(text || `Failed to load films (${resp.status})`);
                }

                const raw = (await resp.json()) as any[];

                if (cancelled) return;

                const mapped: UIMovie[] = raw.map((m) => ({
                    id: m.id,
                    title: m.title,
                    year: m.productionYear,
                    duration: m.duration ?? "",
                    posterImage: m.posterImage ?? "",
                    rating: 0, // TODO: когда появится рейтинг-сервис, заменишь здесь
                }));

                setData(mapped);
            } catch (err: any) {
                if (!cancelled) {
                    setError(err.message ?? "Unknown error");
                }
            } finally {
                if (!cancelled) {
                    setLoading(false);
                }
            }
        }

        load();

        return () => {
            cancelled = true;
        };
    }, []);

    return { data, loading, error };
}
