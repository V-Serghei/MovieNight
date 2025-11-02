"use client"

import { Card, CardContent, CardFooter } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Badge } from "@/components/ui/badge"
import { Play, Bookmark, BookmarkCheck, Star } from "lucide-react"
import type { Movie } from "@/lib/types"
import { useBookmarks } from "@/lib/bookmarks-context"
import { useToast } from "@/hooks/use-toast"

interface MovieCardProps {
    movie: Movie
}

export function MovieCard({ movie }: MovieCardProps) {
    const { bookmarks, addBookmark, removeBookmark } = useBookmarks()
    const { toast } = useToast()
    const isBookmarked = bookmarks.some((b) => b.id === movie.id)

    const handleBookmark = () => {
        if (isBookmarked) {
            removeBookmark(movie.id)
            toast({
                title: "Removed from bookmarks",
                description: `${movie.title} has been removed from your bookmarks.`,
            })
        } else {
            addBookmark(movie)
            toast({
                title: "Added to bookmarks",
                description: `${movie.title} has been added to your bookmarks.`,
            })
        }
    }

    return (
        <Card className="overflow-hidden bg-card/50 backdrop-blur border-border hover:border-primary/50 transition-all group">
            <div className="aspect-[2/3] relative overflow-hidden bg-muted">
                <img
                    src={movie.poster || "/placeholder.svg"}
                    alt={movie.title}
                    className="object-cover w-full h-full group-hover:scale-105 transition-transform duration-300"
                />
                <div className="absolute top-2 right-2">
                    <Badge variant="secondary" className="bg-background/80 backdrop-blur">
                        <Star className="h-3 w-3 mr-1 fill-primary text-primary" />
                        {movie.rating}
                    </Badge>
                </div>
            </div>
            <CardContent className="p-4">
                <h3 className="font-semibold text-lg text-balance leading-tight mb-1">{movie.title}</h3>
                <p className="text-sm text-muted-foreground">
                    {movie.year} • {movie.duration}
                </p>
            </CardContent>
            <CardFooter className="p-4 pt-0 gap-2">
                <Button className="flex-1 gap-2" size="sm">
                    <Play className="h-4 w-4" />
                    Watch
                </Button>
                <Button
                    variant={isBookmarked ? "default" : "outline"}
                    size="icon"
                    onClick={handleBookmark}
                    className={isBookmarked ? "bg-primary text-primary-foreground" : ""}
                >
                    {isBookmarked ? <BookmarkCheck className="h-4 w-4" /> : <Bookmark className="h-4 w-4" />}
                    <span className="sr-only">{isBookmarked ? "Remove from bookmarks" : "Add to bookmarks"}</span>
                </Button>
            </CardFooter>
        </Card>
    )
}
