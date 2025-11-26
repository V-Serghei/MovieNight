import { UIMovie } from "@/lib/types/movie/movie";

export type BookmarkKind = "Normal" | "Temporary" | "Watched";

export interface BookmarkItem {
    id: string;
    movieId: string;
    kind: BookmarkKind;
    createdAt: string;
    watchedAt?: string | null;
    expiresAt?: string | null;
    movie: UIMovie;
}
