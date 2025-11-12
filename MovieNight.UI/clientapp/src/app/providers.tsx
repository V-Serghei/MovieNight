"use client"

import type React from "react"

import { AuthProvider } from "@/lib/auth-context"
import { BookmarksProvider } from "@/lib/bookmarks-context"

export function Providers({ children }: { children: React.ReactNode }) {
    return (
        <AuthProvider>
            <BookmarksProvider>{children}</BookmarksProvider>
        </AuthProvider>
    )
}
