// src/lib/bookmarks-context.tsx
"use client";

import {
    createContext,
    useContext,
    useEffect,
    useState,
    type ReactNode,
} from "react";
import type { BookmarkItem } from "@/lib/types/bookmarks";
import type { UIMovie } from "@/lib/types/movie/movie";
import {
    getBookmarks as apiGetBookmarks,
    addBookmark as apiAddBookmark,
    removeBookmark as apiRemoveBookmark,
} from "@/lib/api/bookmarks";

type BookmarksContextValue = {
    bookmarks: BookmarkItem[];
    loading: boolean;
    error: string | null;
    refresh: () => Promise<void>;
    addBookmark: (movie: UIMovie) => Promise<void>;
    removeBookmark: (movieId: string) => Promise<void>;
};

const BookmarksContext = createContext<BookmarksContextValue | undefined>(
    undefined,
);

export function BookmarksProvider({ children }: { children: ReactNode }) {
    const [bookmarks, setBookmarks] = useState<BookmarkItem[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const load = async () => {
        setLoading(true);
        setError(null);
        try {
            const data = await apiGetBookmarks();
            setBookmarks(data);
        } catch (e: any) {
            setError(e?.message ?? "Failed to load bookmarks");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        void load();
    }, []);

    const addBookmark = async (movie: UIMovie) => {
        const created = await apiAddBookmark(movie);
        setBookmarks((prev) => {
            if (prev.some((b) => b.movieId === movie.id)) return prev;
            return [...prev, created];
        });
    };

    const removeBookmark = async (movieId: string) => {
        await apiRemoveBookmark(movieId);
        setBookmarks((prev) => prev.filter((b) => b.movieId !== movieId));
    };

    const value: BookmarksContextValue = {
        bookmarks,
        loading,
        error,
        refresh: load,
        addBookmark,
        removeBookmark,
    };

    return (
        <BookmarksContext.Provider value={value}>
            {children}
        </BookmarksContext.Provider>
    );
}

export function useBookmarks(): BookmarksContextValue {
    const ctx = useContext(BookmarksContext);
    if (!ctx) {
        throw new Error("useBookmarks must be used within BookmarksProvider");
    }
    return ctx;
}
