"use client"

import { useState } from "react"
import { MovieCard } from "@/components/movie-card"
import { SortDropdown } from "@/components/sort-dropdown"
import type { Movie, SortOption } from "@/lib/types"
import { sortMovies } from "@/lib/utils"
import {UIMovie} from "@/lib/types/movie/movie";

interface MovieGridProps {
    title: string
    movies: UIMovie[]
}

export function MovieGrid({ title, movies }: MovieGridProps) {
    const [sortBy, setSortBy] = useState<SortOption>("rating")
    const sortedMovies = sortMovies(movies, sortBy)

    return (
        <div>
            <div className="flex items-center justify-between mb-8">
                <h1 className="text-4xl md:text-5xl font-serif font-bold">{title}</h1>
                <SortDropdown value={sortBy} onChange={setSortBy} />
            </div>

            {sortedMovies.length === 0 ? (
                <div className="text-center py-12">
                    <p className="text-muted-foreground text-lg">No movies found</p>
                </div>
            ) : (
                <div className="grid gap-6 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4">
                    {sortedMovies.map((movie) => (
                        <MovieCard key={movie.id} movie={movie} />
                    ))}
                </div>
            )}
        </div>
    )
}
