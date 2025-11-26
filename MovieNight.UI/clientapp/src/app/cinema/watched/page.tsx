"use client";

import { TopBar } from "@/components/top-bar";
import { WatchedList } from "@/components/watched-list"; 

export default function WatchedPage() {
    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12 space-y-6">
                <h1 className="text-4xl md:text-5xl font-serif font-bold">
                    Watched movies
                </h1>
                <p className="text-sm text-muted-foreground">
                    Here is the list of movies you have marked as watched.
                </p>
                <WatchedList />
            </main>
        </div>
    );
}
