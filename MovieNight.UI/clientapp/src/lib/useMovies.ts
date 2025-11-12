"use client";
import { useEffect, useState } from "react";
import type { UIMovie } from "@/lib/types/movie/movie";

export function useMoviesByFilms() {
    const [data, setData] = useState<UIMovie[] | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let alive = true;
        (async () => {
            try {
                const res = await fetch("/api/gw/movies/films", { credentials: "include" });
                const text = await res.text();
                if (!res.ok) throw new Error(text || `HTTP ${res.status}`);
                const json = JSON.parse(text) as UIMovie[];
                if (alive) setData(json);
            } catch (e: any) {
                if (alive) setError(e?.message ?? String(e));
            } finally {
                if (alive) setLoading(false);
            }
        })();
        return () => { alive = false; };
    }, []);

    return { data, loading, error };
}
