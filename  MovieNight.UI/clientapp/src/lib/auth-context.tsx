"use client"

import type React from "react"

import { createContext, useContext, useState, useEffect } from "react"
import type { User } from "@/lib/types"

interface AuthContextType {
    user: User | null
    login: (user: User) => void
    logout: () => void
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export function AuthProvider({ children }: { children: React.ReactNode }) {
    const [user, setUser] = useState<User | null>(null)

    useEffect(() => {
        // Load user from localStorage on mount
        const savedUser = localStorage.getItem("movie-night-user")
        if (savedUser) {
            setUser(JSON.parse(savedUser))
        }
    }, [])

    const login = (user: User) => {
        setUser(user)
        localStorage.setItem("movie-night-user", JSON.stringify(user))
    }

    const logout = () => {
        setUser(null)
        localStorage.removeItem("movie-night-user")
    }

    return <AuthContext.Provider value={{ user, login, logout }}>{children}</AuthContext.Provider>
}

export function useAuth() {
    const context = useContext(AuthContext)
    if (context === undefined) {
        throw new Error("useAuth must be used within an AuthProvider")
    }
    return context
}

