"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import type { BookmarkItem } from "@/lib/types/bookmarks";
import { getWatched } from "@/lib/api/bookmarks";
import { MovieCard } from "@/components/movie-card";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

export function WatchedList() {
    const [items, setItems] = useState<BookmarkItem[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const router = useRouter();

    useEffect(() => {
        let cancelled = false;

        (async () => {
            try {
                setLoading(true);
                setError(null);
                const data = await getWatched();
                if (cancelled) return;
                setItems(data);
            } catch (e: any) {
                if (!cancelled) {
                    setError(e?.message ?? "Failed to load watched list");
                }
            } finally {
                if (!cancelled) {
                    setLoading(false);
                }
            }
        })();

        return () => {
            cancelled = true;
        };
    }, []);

    if (loading) {
        return (
            <p className="text-muted-foreground">
                Loading watched…
            </p>
        );
    }

    if (error) {
        return (
            <p className="text-destructive">
                {error}
            </p>
        );
    }

    if (items.length === 0) {
        return (
            <Card className="bg-card/50 backdrop-blur">
                <CardContent className="p-12 text-center space-y-4">
                    <p className="text-muted-foreground">
                        You haven&apos;t marked any movies as watched yet.
                    </p>
                    <Button type="button" onClick={() => router.push("/cinema/films")}>
                        Browse movies
                    </Button>
                </CardContent>
            </Card>
        );
    }

    const movies = items
        .map((w) => w.movie)
        .filter(
            (m): m is NonNullable<BookmarkItem["movie"]> => !!m
        );

    return (
        <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
            {movies.map((m) => (
                <MovieCard key={m.id} movie={m} />
            ))}
        </div>
    );
}
