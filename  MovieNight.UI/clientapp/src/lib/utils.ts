import { type ClassValue } from "clsx"
import { clsx } from "clsx"
import { twMerge } from "tailwind-merge"
import type { Movie, SortOption } from "@/lib/types"

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}
export type SortDirection = "asc" | "desc"
export function sortMovies(
    movies: Movie[],
    option: SortOption,
    direction: SortDirection = "desc"
): Movie[] {
    const arr = [...movies] // не мутируем исходный массив

    const dir = direction === "asc" ? 1 : -1

    return arr.sort((a, b) => {
        switch (option) {
            case "rating": {
                const av = a.rating ?? 0
                const bv = b.rating ?? 0
                return (av - bv) * dir
            }
            case "year": {
                const av = a.year ?? 0
                const bv = b.year ?? 0
                return (av - bv) * dir
            }
            case "alphabet": {
                const av = (a.title ?? "").localeCompare(b.title ?? "", undefined, { sensitivity: "base" })
                return av * (direction === "asc" ? 1 : 1) // для строки просто используем localeCompare; инвертируем ниже
            }
            case "popularity": {
                const av = a.rating ?? 0 // просмотры/лайки — как ты хранишь
                const bv = b.rating ?? 0
                return (av - bv) * dir
            }
            case "duration": {
                const av = a.duration ?? 0 // в минутах
                const bv = b.duration ?? 0
                return (av - bv) * dir
            }
            default:
                return 0
        }
    })
}
