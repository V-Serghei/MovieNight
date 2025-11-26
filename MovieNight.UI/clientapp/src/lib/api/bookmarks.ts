import type { UIMovie } from "@/lib/types/movie/movie";
import type { BookmarkItem } from "@/lib/types/bookmarks";

const BASE = "/api/gw/bookmarks";

type ApiMovieDto = {
    id: string;
    title: string;
    year: number;
    duration?: string | null;
    posterImage?: string | null;
};

function toApiMovie(m: UIMovie): ApiMovieDto {
    return {
        id: m.id,
        title: m.title,
        year: m.year,
        duration: m.duration,
        posterImage: m.posterImage,
    };
}

async function ensureOk(resp: Response, message: string) {
    if (!resp.ok) {
        const text = await resp.text();
        throw new Error(text || message);
    }
}

// --- READ ---

export async function getBookmarks(): Promise<BookmarkItem[]> {
    const resp = await fetch(`${BASE}`, { credentials: "include" });
    await ensureOk(resp, "Failed to load bookmarks");
    return (await resp.json()) as BookmarkItem[];
}

export async function getTemporaryBookmarks(): Promise<BookmarkItem[]> {
    const resp = await fetch(`${BASE}/temp`, { credentials: "include" });
    await ensureOk(resp, "Failed to load temporary bookmarks");
    return (await resp.json()) as BookmarkItem[];
}

export async function getWatched(): Promise<BookmarkItem[]> {
    const resp = await fetch(`${BASE}/watched`, { credentials: "include" });
    await ensureOk(resp, "Failed to load watched list");
    return (await resp.json()) as BookmarkItem[];
}

// --- WRITE ---

export async function addBookmark(movie: UIMovie): Promise<BookmarkItem> {
    const resp = await fetch(`${BASE}`, {
        method: "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ movie: toApiMovie(movie) }),
    });
    await ensureOk(resp, "Failed to add bookmark");
    return (await resp.json()) as BookmarkItem;
}

export async function removeBookmark(movieId: string): Promise<void> {
    const resp = await fetch(`${BASE}/${movieId}`, {
        method: "DELETE",
        credentials: "include",
    });
    await ensureOk(resp, "Failed to remove bookmark");
}

export async function addTemporary(
    movie: UIMovie,
    ttlMinutes?: number,
): Promise<void> {
    const resp = await fetch(`${BASE}/temp`, {
        method: "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            movie: toApiMovie(movie),
            ttlMinutes: ttlMinutes ?? 24 * 60,
        }),
    });
    await ensureOk(resp, "Failed to add temporary bookmark");
}

export async function removeTemporary(movieId: string): Promise<void> {
    const resp = await fetch(`${BASE}/temp/${movieId}`, {
        method: "DELETE",
        credentials: "include",
    });
    await ensureOk(resp, "Failed to remove temporary bookmark");
}

export async function addWatched(
    movie: UIMovie,
    watchedAt?: Date,
): Promise<void> {
    const resp = await fetch(`${BASE}/watched`, {
        method: "POST",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
            movie: toApiMovie(movie),
            watchedAt: watchedAt?.toISOString() ?? null,
        }),
    });
    await ensureOk(resp, "Failed to add watched movie");
}

export async function removeWatched(movieId: string): Promise<void> {
    const resp = await fetch(`${BASE}/watched/${movieId}`, {
        method: "DELETE",
        credentials: "include",
    });
    await ensureOk(resp, "Failed to remove watched movie");
}

export async function deleteAllForMovie(movieId: string): Promise<void> {
    const resp = await fetch(`${BASE}/movie/${movieId}/all`, {
        method: "DELETE",
        credentials: "include",
    });
    await ensureOk(resp, "Failed to delete all bookmarks for movie");
}