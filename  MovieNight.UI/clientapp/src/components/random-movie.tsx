"use client"

import { useState } from "react"
import { Button } from "@/components/ui/button"
import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card"
import { Badge } from "@/components/ui/badge"
import { Shuffle, Play, Bookmark, Star } from "lucide-react"
import { getAllMovies } from "@/lib/mock-data"
import type { Movie } from "@/lib/types"
import { useBookmarks } from "@/lib/bookmarks-context"
import { useToast } from "@/hooks/use-toast"

export function RandomMovie() {
    const [randomMovie, setRandomMovie] = useState<Movie | null>(null)
    const { addBookmark } = useBookmarks()
    const { toast } = useToast()

    const getRandomMovie = () => {
        const allMovies = getAllMovies()
        const random = allMovies[Math.floor(Math.random() * allMovies.length)]
        setRandomMovie(random)
    }

    const handleAddBookmark = () => {
        if (randomMovie) {
            addBookmark(randomMovie)
            toast({
                title: "Added to bookmarks",
                description: `${randomMovie.title} has been added to your bookmarks.`,
            })
        }
    }

    return (
        <div className="max-w-4xl mx-auto">
            <div className="text-center mb-8">
                <h1 className="text-4xl md:text-5xl font-serif font-bold mb-4">Random Film</h1>
                <p className="text-muted-foreground text-lg mb-6">Not sure what to watch? Let us surprise you!</p>
                <Button size="lg" onClick={getRandomMovie} className="gap-2 text-lg px-8">
                    <Shuffle className="h-5 w-5" />
                    Surprise me
                </Button>
            </div>

            {randomMovie && (
                <Card className="overflow-hidden bg-card/50 backdrop-blur border-border">
                    <div className="grid md:grid-cols-2 gap-6">
                        <div className="aspect-[2/3] relative overflow-hidden bg-muted">
                            <img
                                src={randomMovie.poster || "/placeholder.svg"}
                                alt={randomMovie.title}
                                className="object-cover w-full h-full"
                            />
                        </div>
                        <div className="p-6 flex flex-col">
                            <CardHeader className="p-0 mb-4">
                                <div className="flex items-center gap-2 mb-2">
                                    <Badge variant="secondary">
                                        <Star className="h-3 w-3 mr-1 fill-primary text-primary" />
                                        {randomMovie.rating}
                                    </Badge>
                                    <Badge variant="outline">{randomMovie.year}</Badge>
                                    <Badge variant="outline">{randomMovie.duration}</Badge>
                                </div>
                                <h2 className="text-3xl font-serif font-bold text-balance">{randomMovie.title}</h2>
                            </CardHeader>
                            <CardContent className="p-0 flex-1">
                                <p className="text-muted-foreground leading-relaxed">
                                    {randomMovie.description ||
                                        "A captivating story that will keep you on the edge of your seat. Experience cinema at its finest with stunning visuals and compelling performances."}
                                </p>
                            </CardContent>
                            <CardFooter className="p-0 pt-6 gap-3">
                                <Button className="flex-1 gap-2">
                                    <Play className="h-4 w-4" />
                                    Watch Now
                                </Button>
                                <Button variant="outline" className="gap-2 bg-transparent" onClick={handleAddBookmark}>
                                    <Bookmark className="h-4 w-4" />
                                    Add to Bookmarks
                                </Button>
                            </CardFooter>
                        </div>
                    </div>
                </Card>
            )}
        </div>
    )
}
