"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { TopBar } from "@/components/top-bar";
import { Button } from "@/components/ui/button";
import { Card, CardContent } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { ArrowLeft } from "lucide-react";

type CreditRole = "Actor" | "Director" | "Writer" | "Producer" | "Cameo" | "Other";

type MovieCreditView = {
    id: string;
    personId: string;
    fullName: string;
    role: CreditRole;
    characterName?: string;
    order: number;
    profileImagePath?: string;
};

type MovieLite = {
    id: string;
    title: string;
    productionYear: number;
    posterImage?: string;
    genres: string[];
};

const creditRoleNumberToName: Record<number, CreditRole> = {
    1: "Actor",
    2: "Director",
    3: "Writer",
    4: "Producer",
    5: "Cameo",
    99: "Other",
};

const normalizeCreditRole = (role: any): CreditRole => {
    if (typeof role === "string") {
        const r = role as CreditRole;
        if (["Actor", "Director", "Writer", "Producer", "Cameo", "Other"].includes(r)) {
            return r;
        }
    }
    if (typeof role === "number") {
        return creditRoleNumberToName[role] ?? "Other";
    }
    return "Other";
};

export default function FilmFullCastPage() {
    const params = useParams() as { id: string };
    const router = useRouter();

    const [movie, setMovie] = useState<MovieLite | null>(null);
    const [credits, setCredits] = useState<MovieCreditView[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let cancelled = false;

        async function load() {
            try {
                setLoading(true);
                setError(null);

                const [movieResp, creditsResp] = await Promise.all([
                    fetch(`/api/gw/movies/${params.id}`, {
                        credentials: "include",
                    }),
                    fetch(`/api/gw/people/movies/${params.id}/credits`, {
                        credentials: "include",
                    }),
                ]);

                if (!movieResp.ok) {
                    const txt = await movieResp.text();
                    throw new Error(txt || `Failed to load movie (${movieResp.status})`);
                }

                const rawMovie = await movieResp.json();
                if (cancelled) return;

                const genres: string[] = Array.isArray(rawMovie.genres)
                    ? rawMovie.genres
                        .map((g: any) =>
                            typeof g === "string" ? g : g.name ?? "",
                        )
                        .filter(Boolean)
                    : [];

                const m: MovieLite = {
                    id: rawMovie.id,
                    title: rawMovie.title,
                    productionYear: rawMovie.productionYear,
                    posterImage: rawMovie.posterImage ?? "",
                    genres,
                };

                let parsedCredits: MovieCreditView[] = [];
                if (creditsResp.ok) {
                    const rawCredits = await creditsResp.json();
                    parsedCredits = Array.isArray(rawCredits)
                        ? rawCredits
                            .map((c: any) => {
                                const fullName =
                                    c.fullName ??
                                    c.personFullName ??
                                    c.personName ??
                                    c.name ??
                                    "";
                                if (!fullName) return null;

                                const order =
                                    typeof c.order === "number"
                                        ? c.order
                                        : Number(c.order ?? 0) || 0;

                                const roleValue =
                                    typeof c.role === "number"
                                        ? c.role
                                        : Number(c.role ?? 0) || c.role;

                                const profileImageId =
                                    c.personProfileImageId ??
                                    c.profileImageId ??
                                    null;
                                const profileImagePath = profileImageId
                                    ? `/media/${profileImageId}`
                                    : undefined;

                                return {
                                    id: c.id ?? c.creditId ?? "",
                                    personId: c.personId ?? "",
                                    fullName,
                                    role: normalizeCreditRole(roleValue),
                                    characterName: c.characterName ?? "",
                                    order,
                                    profileImagePath,
                                } as MovieCreditView;
                            })
                            .filter((x): x is MovieCreditView => x !== null)
                        : [];
                }

                if (!cancelled) {
                    setMovie(m);
                    setCredits(parsedCredits);
                }
            } catch (err: any) {
                if (!cancelled) {
                    setError(err.message ?? "Unknown error");
                }
            } finally {
                if (!cancelled) {
                    setLoading(false);
                }
            }
        }

        load();

        return () => {
            cancelled = true;
        };
    }, [params.id]);

    const directors = credits.filter((c) => c.role === "Director");
    const writers = credits.filter((c) => c.role === "Writer");
    const producers = credits.filter((c) => c.role === "Producer");
    const actors = credits
        .filter((c) => c.role === "Actor")
        .slice()
        .sort((a, b) => {
            if (a.order && b.order && a.order !== b.order) return a.order - b.order;
            return a.fullName.localeCompare(b.fullName);
        });
    const cameos = credits.filter((c) => c.role === "Cameo");
    const others = credits.filter((c) => c.role === "Other");

    if (loading) {
        return (
            <div className="min-h-screen">
                <TopBar />
                <main className="container mx-auto px-4 pt-24 pb-12">
                    <p className="text-muted-foreground">Loading cast…</p>
                </main>
            </div>
        );
    }

    if (error || !movie) {
        return (
            <div className="min-h-screen">
                <TopBar />
                <main className="container mx-auto px-4 pt-24 pb-12 space-y-4">
                    <Button
                        variant="ghost"
                        className="gap-2"
                        type="button"
                        onClick={() => router.back()}
                    >
                        <ArrowLeft className="h-4 w-4" />
                        Back
                    </Button>
                    <p className="text-destructive">
                        {error ?? "Movie not found"}
                    </p>
                </main>
            </div>
        );
    }

    const renderSection = (title: string, items: MovieCreditView[]) => {
        if (items.length === 0) return null;

        return (
            <section className="space-y-3">
                <h2 className="text-lg font-semibold">{title}</h2>
                <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-3">
                    {items.map((c) => (
                        <Card
                            key={c.id || `${c.fullName}-${c.order}-${title}`}
                            className="bg-card/40 border-border overflow-hidden flex gap-3 p-3"
                        >
                            <div className="w-12 h-12 rounded-full bg-muted overflow-hidden flex items-center justify-center text-xs flex-shrink-0">
                                {c.profileImagePath ? (
                                    <img
                                        src={`/api/gw${c.profileImagePath}`}
                                        alt={c.fullName}
                                        className="w-full h-full object-cover"
                                    />
                                ) : (
                                    <span className="text-muted-foreground">
                                        {c.fullName
                                            .split(" ")
                                            .map((x) => x[0])
                                            .join("")
                                            .slice(0, 2)
                                            .toUpperCase()}
                                    </span>
                                )}
                            </div>
                            <CardContent className="p-0 flex-1 space-y-1">
                                <p className="text-sm font-semibold">
                                    {c.fullName}
                                </p>
                                {c.characterName && (
                                    <p className="text-xs text-muted-foreground">
                                        as{" "}
                                        <span className="italic">
                                            {c.characterName}
                                        </span>
                                    </p>
                                )}
                                {c.order > 0 && (
                                    <p className="text-[11px] text-muted-foreground">
                                        Order: #{c.order}
                                    </p>
                                )}
                            </CardContent>
                        </Card>
                    ))}
                </div>
            </section>
        );
    };

    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12 space-y-6">
                <Button
                    variant="ghost"
                    className="gap-2"
                    type="button"
                    onClick={() => router.back()}
                >
                    <ArrowLeft className="h-4 w-4" />
                    Back
                </Button>

                <header className="flex flex-col md:flex-row gap-4 md:items-center">
                    <div className="relative w-24 h-36 rounded-lg bg-muted overflow-hidden flex-shrink-0">
                        {movie.posterImage ? (
                            <img
                                src={`/api/gw${movie.posterImage}`}
                                alt={movie.title}
                                className="w-full h-full object-cover"
                            />
                        ) : (
                            <div className="w-full h-full flex items-center justify-center text-xs text-muted-foreground">
                                No poster
                            </div>
                        )}
                    </div>
                    <div className="space-y-1">
                        <h1 className="text-2xl md:text-3xl font-serif font-bold">
                            {movie.title}
                        </h1>
                        <p className="text-sm text-muted-foreground">
                            {movie.productionYear}
                        </p>
                        {movie.genres.length > 0 && (
                            <div className="flex flex-wrap gap-2 mt-2">
                                {movie.genres.map((g) => (
                                    <Badge key={g} variant="outline">
                                        {g}
                                    </Badge>
                                ))}
                            </div>
                        )}
                    </div>
                </header>

                <div className="space-y-8">
                    {renderSection("Directors", directors)}
                    {renderSection("Writers", writers)}
                    {renderSection("Producers", producers)}
                    {renderSection("Cast", actors)}
                    {renderSection("Cameos", cameos)}
                    {renderSection("Other", others)}
                </div>
            </main>
        </div>
    );
}
