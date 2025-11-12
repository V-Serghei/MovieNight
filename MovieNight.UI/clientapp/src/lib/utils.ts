import { type ClassValue } from "clsx"
import { clsx } from "clsx"
import { twMerge } from "tailwind-merge"
import type { Movie, SortOption } from "@/lib/types"
import {UIMovie} from "@/lib/types/movie/movie";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs))
}
export type SortDirection = "asc" | "desc"

const toNumber = (v: unknown): number => {
    if (v == null) return 0
    if (typeof v === "number") return Number.isFinite(v) ? v : 0
    if (typeof v === "string") {
        // вычистим всё кроме цифр, знака, точки/запятой
        const cleaned = v.trim().replace(/[^\d.,-]/g, "").replace(",", ".")
        const n = Number.parseFloat(cleaned)
        return Number.isFinite(n) ? n : 0
    }
    const n = Number(v as any)
    return Number.isFinite(n) ? n : 0
}

const toYear = (v: unknown): number => {
    if (v == null) return 0
    if (typeof v === "number") return v
    if (v instanceof Date) return v.getUTCFullYear()
    if (typeof v === "string") {
        // попробуем найти 4 цифры (год)
        const m = v.match(/\b(\d{4})\b/)
        if (m) return +m[1]
        const n = Number.parseInt(v, 10)
        return Number.isFinite(n) ? n : 0
    }
    return 0
}

const toMinutes = (v: unknown): number => {
    if (v == null) return 0
    if (typeof v === "number") return v
    if (typeof v === "string") {
        const s = v.trim()

        // HH:MM[:SS]
        if (s.includes(":")) {
            const parts = s.split(":").map((p) => Number.parseInt(p, 10) || 0)
            const [hh = 0, mm = 0, ss = 0] = parts
            return hh * 60 + mm + Math.floor(ss / 60)
        }

        // "2h 30m", "2h", "150m"
        const h = s.match(/(\d+(?:\.\d+)?)\s*h/i)
        const m = s.match(/(\d+(?:\.\d+)?)\s*m/i)
        if (h || m) {
            const hours = h ? parseFloat(h[1]) : 0
            const mins = m ? parseFloat(m[1]) : 0
            return Math.round(hours * 60 + mins)
        }

        // просто число в строке → минуты
        return toNumber(s)
    }
    return 0
}
export function sortMovies(
    movies: UIMovie[],
    option: SortOption,
    direction: SortDirection = "desc"
): UIMovie[] {
    const arr = [...movies]

    const dir = direction === "asc" ? 1 : -1

    return arr.sort((a, b) => {
        switch (option) {
            case "rating": {
                const av = toNumber((a as any).rating)
                const bv = toNumber((b as any).rating)
                return (av - bv) * dir
            }
            case "year": {
                const av = toYear((a as any).year ?? (a as any).productionYear)
                const bv = toYear((b as any).year ?? (b as any).productionYear)
                return (av - bv) * dir
            }
            case "alphabet": {
                const av = (a.title ?? "")
                const bv = (b.title ?? "")
                const cmp = av.localeCompare(bv, undefined, { sensitivity: "base" })
                return cmp * dir
            }
            case "popularity": {
                // если есть отдельное поле popularity — поменяй здесь
                const av = toNumber((a as any).popularity ?? (a as any).rating)
                const bv = toNumber((b as any).popularity ?? (b as any).rating)
                return (av - bv) * dir
            }
            case "duration": {
                const av = toMinutes((a as any).duration)
                const bv = toMinutes((b as any).duration)
                return (av - bv) * dir
            }
            default:
                return 0
        }
    })
}
