"use client";

import Link from "next/link";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Bookmark } from "lucide-react";
import { useBookmarks } from "@/lib/bookmarks-context";
import { MovieCard } from "@/components/movie-card";
import type { BookmarkItem } from "@/lib/types/bookmarks";
import type { UIMovie } from "@/lib/types/movie/movie";

function bookmarkToUIMovie(b: BookmarkItem): UIMovie {
    const m = b.movie ?? {
        id: b.movieId,
        title: "Unknown",
        year: 0,
        duration: "",
        posterImage: "",
        rating: 0,
    };

    return {
        id: m.id ?? b.movieId,
        title: m.title ?? "Unknown",
        year: m.year ?? 0,
        duration: m.duration ?? "",
        posterImage: m.posterImage ?? "",
        rating: m.rating ?? 0,
    };
}

export function BookmarksList() {
    const { bookmarks } = useBookmarks();

    return (
        <div>
            <h1 className="text-4xl md:text-5xl font-serif font-bold mb-8">
                My Bookmarks
            </h1>

            {bookmarks.length === 0 ? (
                <Card className="bg-card/50 backdrop-blur">
                    <CardContent className="p-12 text-center">
                        <Bookmark className="h-12 w-12 mx-auto mb-4 text-muted-foreground" />
                        <p className="text-muted-foreground mb-4">
                            You haven't bookmarked any movies yet
                        </p>
                        <Button asChild>
                            <Link href="/cinema/films">Browse Movies</Link>
                        </Button>
                    </CardContent>
                </Card>
            ) : (
                <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                    {bookmarks.map((item) => {
                        const movie = bookmarkToUIMovie(item);
                        return <MovieCard key={movie.id} movie={movie} />;
                    })}
                </div>
            )}
        </div>
    );
}
