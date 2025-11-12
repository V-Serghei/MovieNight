"use client"

import type React from "react"

import { createContext, useContext, useState, useEffect } from "react"
import type { Movie } from "@/lib/types"
import {UIMovie} from "@/lib/types/movie/movie";

interface BookmarksContextType {
    bookmarks: UIMovie[]
    addBookmark: (movie: UIMovie) => void
    removeBookmark: (id: string) => void
}

const BookmarksContext = createContext<BookmarksContextType | undefined>(undefined)

export function BookmarksProvider({ children }: { children: React.ReactNode }) {
    const [bookmarks, setBookmarks] = useState<UIMovie[]>([])

    useEffect(() => {
        // Load bookmarks from localStorage on mount
        const savedBookmarks = localStorage.getItem("movie-night-bookmarks")
        if (savedBookmarks) {
            setBookmarks(JSON.parse(savedBookmarks))
        }
    }, [])

    const addBookmark = (movie: UIMovie) => {
        setBookmarks((prev) => {
            const updated = [...prev, movie]
            localStorage.setItem("movie-night-bookmarks", JSON.stringify(updated))
            return updated
        })
    }

    const removeBookmark = (id: string) => {
        setBookmarks((prev) => {
            const updated = prev.filter((m) => m.id !== id)
            localStorage.setItem("movie-night-bookmarks", JSON.stringify(updated))
            return updated
        })
    }

    return (
        <BookmarksContext.Provider value={{ bookmarks, addBookmark, removeBookmark }}>{children}</BookmarksContext.Provider>
    )
}

export function useBookmarks() {
    const context = useContext(BookmarksContext)
    if (context === undefined) {
        throw new Error("useBookmarks must be used within a BookmarksProvider")
    }
    return context
}
