// src/components/temp-bookmarks-dropdown.tsx
"use client";

import { useEffect, useState } from "react";
import { Timer } from "lucide-react";
import { Button } from "@/components/ui/button";
import {
    Popover,
    PopoverTrigger,
    PopoverContent,
} from "@/components/ui/popover";
import type { BookmarkItem } from "@/lib/types/bookmarks";
import {
    getTemporaryBookmarks,
    removeTemporary,
} from "@/lib/api/bookmarks";
import { useRouter } from "next/navigation";

export function TempBookmarksDropdown() {
    const [open, setOpen] = useState(false);
    const [items, setItems] = useState<BookmarkItem[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const router = useRouter();

    const load = async () => {
        setLoading(true);
        setError(null);
        try {
            const data = await getTemporaryBookmarks();
            setItems(data);
        } catch (e: any) {
            setError(e?.message ?? "Failed to load temporary bookmarks");
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        if (open) {
            void load();
        }
    }, [open]);

    const handleRemove = async (movieId: string) => {
        try {
            await removeTemporary(movieId);
            setItems((prev) => prev.filter((x) => x.movieId !== movieId));
        } catch {
        }
    };

    const openMovie = (movieId: string) => {
        setOpen(false);
        router.push(`/cinema/${movieId}`);
    };

    return (
        <Popover open={open} onOpenChange={setOpen}>
            <PopoverTrigger asChild>
                <Button
                    variant="ghost"
                    size="icon"
                    className="hover:text-primary"
                    title="Temporary bookmarks"
                >
                    <Timer className="h-5 w-5" />
                    <span className="sr-only">Temporary bookmarks</span>
                </Button>
            </PopoverTrigger>
            <PopoverContent className="w-80 p-2 space-y-2">
                <div className="text-sm font-medium">Temporary bookmarks</div>

                {loading && (
                    <p className="text-xs text-muted-foreground">Loading…</p>
                )}

                {error && !loading && (
                    <p className="text-xs text-destructive">{error}</p>
                )}

                {!loading && !error && items.length === 0 && (
                    <p className="text-xs text-muted-foreground">
                        No temporary bookmarks.
                    </p>
                )}

                <div className="space-y-2 max-h-80 overflow-auto">
                    {items.map((b) => (
                        <div
                            key={b.id}
                            className="flex items-center gap-2 text-xs border rounded-md p-1 bg-card/40 hover:border-primary/60 cursor-pointer"
                            onClick={() => openMovie(b.movieId)}
                        >
                            <div className="w-12 h-16 overflow-hidden rounded bg-muted flex-shrink-0">
                                {b.movie.posterImage && (
                                    <img
                                        src={`/api/gw${b.movie.posterImage}`}
                                        alt={b.movie.title}
                                        className="w-full h-full object-cover"
                                    />
                                )}
                            </div>
                            <div className="flex-1 min-w-0">
                                <div className="font-semibold truncate">
                                    {b.movie.title}
                                </div>
                                <div className="text-[11px] text-muted-foreground">
                                    {b.movie.year} • {b.movie.duration}
                                </div>
                                {b.expiresAt && (
                                    <div className="text-[10px] text-muted-foreground">
                                        until{" "}
                                        {new Date(
                                            b.expiresAt,
                                        ).toLocaleString()}
                                    </div>
                                )}
                            </div>
                            <Button
                                variant="ghost"
                                size="icon-sm"
                                type="button"
                                onClick={(e) => {
                                    e.stopPropagation();
                                    void handleRemove(b.movieId);
                                }}
                            >
                                ×
                            </Button>
                        </div>
                    ))}
                </div>
            </PopoverContent>
        </Popover>
    );
}
