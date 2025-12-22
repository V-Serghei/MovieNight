// src/components/movie-card.tsx
"use client";

import { useState } from "react";
import { Card, CardContent, CardFooter } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Play, Bookmark, BookmarkCheck, Star, Eye, Clock } from "lucide-react";
import { useRouter } from "next/navigation";
import { useBookmarks } from "@/lib/bookmarks-context";
import { useToast } from "@/hooks/use-toast";
import type { UIMovie } from "@/lib/types/movie/movie";

interface MovieCardProps {
    movie: UIMovie;
}

export function MovieCard({ movie }: MovieCardProps) {
    const router = useRouter();
    const { bookmarks, addBookmark, removeBookmark } = useBookmarks();
    const { toast } = useToast();

    const isBookmarked = bookmarks.some((b) => b.movieId === movie.id);

    const [isWatched, setIsWatched] = useState(false);
    const [isTemp, setIsTemp] = useState(false);

    const openDetails = () => {
        router.push(`/cinema/${movie.id}`);
    };

    const handleBookmark = () => {
        if (isBookmarked) {
            void removeBookmark(movie.id);
            toast({
                title: "Removed from bookmarks",
                description: `${movie.title} has been removed from your bookmarks.`,
            });
        } else {
            void addBookmark(movie);
            toast({
                title: "Added to bookmarks",
                description: `${movie.title} has been added to your bookmarks.`,
            });
        }
    };

    const handleMarkWatched = async (e: React.MouseEvent) => {
        e.stopPropagation();
        try {
            await fetch("/api/gw/bookmarks/watched", {
                method: "POST",
                credentials: "include",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    movie,
                    watchedAt: new Date().toISOString(),
                }),
            });

            setIsWatched(true);

            toast({
                title: "Marked as watched",
                description: `"${movie.title}" has been added to your watched list.`,
            });
        } catch (err: any) {
            toast({
                title: "Error",
                description: err?.message ?? "Failed to mark as watched",
                variant: "destructive",
            });
        }
    };

    const handleTempBookmark = async (e: React.MouseEvent) => {
        e.stopPropagation();
        try {
            await fetch("/api/gw/bookmarks/temp", {
                method: "POST",
                credentials: "include",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    movie,
                    ttlMinutes: 60,
                }),
            });

            setIsTemp(true);

            toast({
                title: "Temporary bookmark added",
                description: `"${movie.title}" added to temporary bookmarks.`,
            });
        } catch (err: any) {
            toast({
                title: "Error",
                description: err?.message ?? "Failed to add temp bookmark",
                variant: "destructive",
            });
        }
    };

    const globalRating =
        typeof movie.rating === "number" && movie.rating > 0
            ? movie.rating.toFixed(1)
            : "—";

    return (
        <Card className="overflow-hidden bg-card/50 backdrop-blur border-border hover:border-primary/50 transition-all group">
            <div
                className="aspect-[2/3] relative overflow-hidden bg-muted cursor-pointer"
                onClick={openDetails}
            >
                <img
                    src={`/api/gw${movie.posterImage}`}
                    alt={movie.title}
                    className="w-full h-full object-cover group-hover:scale-105 transition-transform"
                />

                <div className="absolute top-2 right-2 flex flex-col items-end gap-1">
                    <Badge
                        variant="secondary"
                        className="bg-background/80 backdrop-blur flex items-center"
                    >
                        <Star className="h-3 w-3 mr-1 fill-primary text-primary" />
                        {globalRating}
                    </Badge>

                    {movie.userRating != null && (
                        <Badge
                            variant="secondary"
                            className="bg-background/80 backdrop-blur"
                        >
                            You: {movie.userRating}
                        </Badge>
                    )}
                </div>
            </div>

            <CardContent
                className="p-4 cursor-pointer"
                onClick={openDetails}
            >
                <h3 className="font-semibold text-lg text-balance leading-tight mb-1">
                    {movie.title}
                </h3>
                <p className="text-sm text-muted-foreground">
                    {movie.year} • {movie.duration}
                </p>
            </CardContent>

            <CardFooter className="p-4 pt-0 gap-2">
                <Button
                    className="flex-1 gap-2"
                    size="sm"
                    type="button"
                    onClick={(e) => {
                        e.stopPropagation();
                        // TODO: Player
                    }}
                >
                    <Play className="h-4 w-4" />
                    Watch
                </Button>

                <Button
                    variant={isBookmarked ? "default" : "outline"}
                    size="icon"
                    type="button"
                    onClick={(e) => {
                        e.stopPropagation();
                        handleBookmark();
                    }}
                    className={isBookmarked ? "bg-primary text-primary-foreground" : ""}
                >
                    {isBookmarked ? (
                        <BookmarkCheck className="h-4 w-4" />
                    ) : (
                        <Bookmark className="h-4 w-4" />
                    )}
                    <span className="sr-only">
                        {isBookmarked ? "Remove from bookmarks" : "Add to bookmarks"}
                    </span>
                </Button>

                <Button
                    variant={isWatched ? "default" : "ghost"}
                    size="icon"
                    type="button"
                    onClick={handleMarkWatched}
                >
                    <Eye className="h-4 w-4" />
                    <span className="sr-only">Mark as watched</span>
                </Button>

                <Button
                    variant={isTemp ? "default" : "ghost"}
                    size="icon"
                    type="button"
                    onClick={handleTempBookmark}
                >
                    <Clock className="h-4 w-4" />
                    <span className="sr-only">Add temporary bookmark</span>
                </Button>
            </CardFooter>
        </Card>
    );
}
