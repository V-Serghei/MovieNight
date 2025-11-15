"use client";

import { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Card, CardContent } from "@/components/ui/card";
import { Tabs, TabsList, TabsTrigger, TabsContent } from "@/components/ui/tabs";
import { Film, Users, ShieldCheck } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useCurrentProfile } from "@/lib/use-current-profile";

type MovieSummary = {
    id: string;
    title: string;
};

export default function AdminPage() {
    const router = useRouter();
    const { profile, loading, hasAdminAccess } = useCurrentProfile();
    const [moviesCount, setMoviesCount] = useState<number | null>(null);
    const [loadingMovies, setLoadingMovies] = useState(false);

    useEffect(() => {
        if (!hasAdminAccess) return;

        let cancelled = false;

        const run = async () => {
            setLoadingMovies(true);
            try {
                const res = await fetch("/api/gw/movies", {
                    credentials: "include",
                });

                if (!res.ok) {
                    throw new Error("Failed to load movies");
                }

                const data = (await res.json()) as MovieSummary[];
                if (!cancelled) {
                    setMoviesCount(data.length);
                }
            } catch {
                if (!cancelled) {
                    setMoviesCount(null);
                }
            } finally {
                if (!cancelled) {
                    setLoadingMovies(false);
                }
            }
        };

        run();
        return () => {
            cancelled = true;
        };
    }, [hasAdminAccess]);

    if (loading) {
        return (
            <div className="max-w-6xl mx-auto">
                <h1 className="text-4xl md:text-5xl font-serif font-bold mb-8">
                    Admin
                </h1>
                <Card className="bg-card/50 backdrop-blur border-border">
                    <CardContent className="p-8">
                        <p className="text-muted-foreground">Loading…</p>
                    </CardContent>
                </Card>
            </div>
        );
    }

    if (!profile || !hasAdminAccess) {
        return (
            <div className="max-w-4xl mx-auto">
                <h1 className="text-4xl md:text-5xl font-serif font-bold mb-8">
                    Admin
                </h1>
                <Card className="bg-card/50 backdrop-blur border-border">
                    <CardContent className="p-8 text-center space-y-3">
                        <ShieldCheck className="h-8 w-8 mx-auto text-muted-foreground" />
                        <p className="text-muted-foreground">
                            You don&apos;t have access to the admin area.
                        </p>
                    </CardContent>
                </Card>
            </div>
        );
    }

    return (
        <div className="max-w-6xl mx-auto">
            <h1 className="text-4xl md:text-5xl font-serif font-bold mb-8">
                Admin
            </h1>

            <Card className="bg-card/50 backdrop-blur border-border">
                <CardContent className="p-6 flex gap-6">
                    <Tabs
                        defaultValue="movies"
                        orientation="vertical"
                        className="flex gap-6 w-full"
                    >
                        <TabsList className="flex flex-col w-48 h-fit">
                            <TabsTrigger
                                value="movies"
                                className="justify-start gap-2"
                            >
                                <Film className="h-4 w-4" />
                                <span>Movies</span>
                            </TabsTrigger>

                            <TabsTrigger
                                value="users"
                                className="justify-start gap-2"
                                disabled
                            >
                                <Users className="h-4 w-4" />
                                <span>Users (soon)</span>
                            </TabsTrigger>
                        </TabsList>

                        <div className="flex-1">
                            <TabsContent value="movies" className="space-y-6">
                                {/* Статистика по фильмам */}
                                <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                                    <div className="p-4 rounded-lg bg-muted/50 border border-border">
                                        <div className="flex items-center gap-3 mb-2">
                                            <Film className="h-5 w-5 text-primary" />
                                            <span className="font-semibold">
                                                Total movies
                                            </span>
                                        </div>
                                        <p className="text-2xl font-bold">
                                            {loadingMovies || moviesCount === null
                                                ? "…"
                                                : moviesCount}
                                        </p>
                                    </div>
                                </div>

                                {/* Управление фильмами */}
                                <div className="flex items-center justify-between">
                                    <div className="space-y-1">
                                        <h2 className="text-xl font-semibold">
                                            Movies management
                                        </h2>
                                        <p className="text-sm text-muted-foreground">
                                            Add new movies and manage the catalog.
                                        </p>
                                    </div>
                                    <Button
                                        className="gap-2"
                                        onClick={() =>
                                            router.push("/admin/movies/new")
                                        }
                                    >
                                        <Film className="h-4 w-4" />
                                        <span>Add movie</span>
                                    </Button>
                                </div>
                            </TabsContent>
                        </div>
                    </Tabs>
                </CardContent>
            </Card>
        </div>
    );
}
