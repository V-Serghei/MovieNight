export interface Movie {
    id: string
    title: string
    poster: string
    rating: number
    year: number
    duration: number
    description?: string
}

export interface News {
    id: string
    title: string
    excerpt: string
    image: string
    date: string
}

export interface Message {
    id: string
    sender: string
    subject: string
    preview: string
    body: string
    date: string
    read: boolean
}

export interface User {
    id: string
    name: string
    email: string
}

export type SortOption = "rating" | "year" | "alphabet" | "popularity" | "duration"
