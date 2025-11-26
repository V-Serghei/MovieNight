"use client";

import { useEffect, useState } from "react";
import { useParams, useRouter } from "next/navigation";
import { TopBar } from "@/components/top-bar";
import { Badge } from "@/components/ui/badge";
import { Card, CardContent } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { useBookmarks } from "@/lib/bookmarks-context";
import { useToast } from "@/hooks/use-toast";
import type { UIMovie } from "@/lib/types/movie/movie";
import { RatingStars } from "@/components/rating-stars";
import { getMovieRating } from "@/lib/api/ratings";
import {
    rateMovieWithWatched,
    removeRatingAndCleanup,
} from "@/lib/api/rating-bookmarks-orchestrator";
import {
    ArrowLeft,
    Bookmark,
    BookmarkCheck,
    Play,
    Eye,
    Clock,
} from "lucide-react";

type MovieCardDetails = {
    title?: string;
    imageUrl: string;
    description?: string;
};

type MovieFactDetails = {
    factName: string;
    text?: string;
};

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

type MovieDetails = {
    id: string;
    title: string;
    category?: string;
    posterImage?: string;
    quote?: string;
    description?: string;
    productionYear: number;
    country?: string;
    language?: string;
    director?: string;
    duration?: string;
    certificate?: string;
    productionCompany?: string;
    budget?: string;
    grossWorldwide?: string;
    genres: string[];
    cards: MovieCardDetails[];
    facts: MovieFactDetails[];
    credits: MovieCreditView[];
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

export default function FilmDetailsPage() {
    const params = useParams() as { id: string };
    const router = useRouter();
    const { bookmarks, addBookmark, removeBookmark } = useBookmarks();

    const [movie, setMovie] = useState<MovieDetails | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const [watched, setWatched] = useState(false);
    const [tempBookmarked, setTempBookmarked] = useState(false);

    const isBookmarked =
        !!movie && bookmarks.some((b) => b.movieId === movie.id);

    const [ratingSummary, setRatingSummary] = useState<{
        averageRating: number | null;
        ratingsCount: number;
        userRating: number | null;
    } | null>(null);
    const [ratingLoading, setRatingLoading] = useState(false);
    const [ratingError, setRatingError] = useState<string | null>(null);

    const { toast } = useToast();

    const toUiMovie = (m: MovieDetails): UIMovie => ({
        id: m.id,
        title: m.title,
        year: m.productionYear,
        duration: m.duration ?? "",
        posterImage: m.posterImage ?? "",
        rating: 0,
    });

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
                    const text = await movieResp.text();
                    throw new Error(text || `Failed to load movie (${movieResp.status})`);
                }

                const raw = await movieResp.json();
                if (cancelled) return;

                // --- genres ---
                const genres: string[] = Array.isArray(raw.genres)
                    ? raw.genres
                        .map((g: any) =>
                            typeof g === "string" ? g : g.name ?? "",
                        )
                        .filter(Boolean)
                    : [];

                // --- cards ---
                const rawCards = raw.movieCards ?? raw.cards ?? [];
                const cards: MovieCardDetails[] = Array.isArray(rawCards)
                    ? rawCards
                        .map((c: any) => ({
                            title: c.title ?? "",
                            imageUrl: c.imageUrl ?? "",
                            description: c.description ?? "",
                        }))
                        .filter((c) => c.imageUrl)
                    : [];

                // --- facts ---
                const rawFacts = raw.interestingFacts ?? raw.facts ?? [];
                const facts: MovieFactDetails[] = Array.isArray(rawFacts)
                    ? rawFacts
                        .map((f: any) => ({
                            factName: f.factName ?? "",
                            text: f.text ?? "",
                        }))
                        .filter((f) => f.factName)
                    : [];

                // --- credits (cast & crew) ---
                let credits: MovieCreditView[] = [];
                if (creditsResp.ok) {
                    const rawCredits = await creditsResp.json();
                    credits = Array.isArray(rawCredits)
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

                const mapped: MovieDetails = {
                    id: raw.id,
                    title: raw.title,
                    category: raw.categoryName ?? raw.category ?? "",
                    posterImage: raw.posterImage ?? "",
                    quote: raw.quote ?? "",
                    description: raw.description ?? "",
                    productionYear: raw.productionYear,
                    country: raw.country ?? "",
                    language: raw.language ?? "",
                    director: raw.director ?? "",
                    duration: raw.duration ?? "",
                    certificate: raw.certificate ?? "",
                    productionCompany: raw.productionCompany ?? "",
                    budget: raw.budget ?? "",
                    grossWorldwide: raw.grossWorldwide ?? "",
                    genres,
                    cards,
                    facts,
                    credits,
                };

                if (!cancelled) {
                    setMovie(mapped);
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

    useEffect(() => {
        let cancelled = false;

        async function loadRating() {
            if (!movie) return;
            try {
                setRatingLoading(true);
                setRatingError(null);
                const summary = await getMovieRating(movie.id);
                if (cancelled) return;
                setRatingSummary({
                    averageRating: summary.averageRating,
                    ratingsCount: summary.ratingsCount,
                    userRating: summary.userRating,
                });
            } catch (e: any) {
                if (!cancelled) {
                    setRatingError(e?.message ?? "Failed to load rating");
                }
            } finally {
                if (!cancelled) {
                    setRatingLoading(false);
                }
            }
        }

        loadRating();

        return () => {
            cancelled = true;
        };
    }, [movie?.id]);

    const handleBookmark = () => {
        if (!movie) return;

        if (isBookmarked) {
            removeBookmark(movie.id);
            toast({
                title: "Removed from bookmarks",
                description: `${movie.title} has been removed from your bookmarks.`,
            });
        } else {
            addBookmark(toUiMovie(movie));
            toast({
                title: "Added to bookmarks",
                description: `${movie.title} has been added to your bookmarks.`,
            });
        }
    };

    const directors = movie
        ? movie.credits
            .filter((c) => c.role === "Director")
            .slice()
            .sort((a, b) => {
                if (a.order && b.order && a.order !== b.order) return a.order - b.order;
                return a.fullName.localeCompare(b.fullName);
            })
        : [];

    const actors = movie
        ? movie.credits
            .filter((c) => c.role === "Actor")
            .slice()
            .sort((a, b) => {
                if (a.order && b.order && a.order !== b.order) return a.order - b.order;
                return a.fullName.localeCompare(b.fullName);
            })
        : [];

    const topCast = [...directors, ...actors].slice(0, 5);

    const handleMarkWatched = async () => {
        if (!movie) return;
        const uiMovie = toUiMovie(movie);
        try {
            await fetch("/api/gw/bookmarks/watched", {
                method: "POST",
                credentials: "include",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    movie: uiMovie,
                    watchedAt: new Date().toISOString(),
                }),
            });
            setWatched(true);
            toast({
                title: "Marked as watched",
                description: `"${movie.title}" has been added to your watched list.`,
            });
        } catch (e: any) {
            toast({
                title: "Error",
                description: e?.message ?? "Failed to mark as watched",
                variant: "destructive",
            });
        }
    };

    const handleTempBookmark = async () => {
        if (!movie) return;
        const uiMovie = toUiMovie(movie);
        try {
            await fetch("/api/gw/bookmarks/temp", {
                method: "POST",
                credentials: "include",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    movie: uiMovie,
                    ttlMinutes: 60,
                }),
            });
            setTempBookmarked(true);
            toast({
                title: "Temporary bookmark added",
                description: `"${movie.title}" added to temporary bookmarks.`,
            });
        } catch (e: any) {
            toast({
                title: "Error",
                description: e?.message ?? "Failed to add temp bookmark",
                variant: "destructive",
            });
        }
    };

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

    const handleSetRating = async (score: number) => {
        if (!movie) return;
        try {
            setRatingLoading(true);
            const uiMovie = toUiMovie(movie);
            const summary = await rateMovieWithWatched(uiMovie, score);
            setRatingSummary({
                averageRating: summary.averageRating,
                ratingsCount: summary.ratingsCount,
                userRating: summary.userRating,
            });
            toast({
                title: "Rating saved",
                description: `You rated "${movie.title}" with ${score}/10.`,
            });
        } catch (e: any) {
            toast({
                title: "Error",
                description: e?.message ?? "Failed to set rating",
                variant: "destructive",
            });
        } finally {
            setRatingLoading(false);
        }
    };

    const handleDeleteRating = async () => {
        if (!movie) return;
        try {
            setRatingLoading(true);
            const uiMovie = toUiMovie(movie);
            const summary = await removeRatingAndCleanup(uiMovie);
            setRatingSummary({
                averageRating: summary.averageRating,
                ratingsCount: summary.ratingsCount,
                userRating: summary.userRating,
            });
            toast({
                title: "Rating removed",
                description: `Your rating for "${movie.title}" has been removed, and history was cleaned up.`,
            });
        } catch (e: any) {
            toast({
                title: "Error",
                description: e?.message ?? "Failed to remove rating",
                variant: "destructive",
            });
        } finally {
            setRatingLoading(false);
        }
    };

    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12">
                <Button
                    variant="ghost"
                    className="gap-2 mb-6"
                    type="button"
                    onClick={() => router.back()}
                >
                    <ArrowLeft className="h-4 w-4" />
                    Back
                </Button>

                <div className="grid gap-8 lg:grid-cols-[minmax(0,2fr)_minmax(0,3fr)]">
                    {/* Левая колонка: постер + кнопки */}
                    <div className="space-y-4">
                        <div className="relative overflow-hidden rounded-xl bg-muted aspect-[2/3]">
                            {movie.posterImage ? (
                                <img
                                    src={`/api/gw${movie.posterImage}`}
                                    alt={movie.title}
                                    className="w-full h-full object-cover"
                                />
                            ) : (
                                <div className="w-full h-full flex items-center justify-center text-muted-foreground text-sm">
                                    No poster
                                </div>
                            )}
                            <div className="absolute top-3 left-3 flex flex-wrap gap-2">
                                {movie.genres.slice(0, 3).map((g) => (
                                    <Badge
                                        key={g}
                                        variant="secondary"
                                        className="bg-background/80 backdrop-blur"
                                    >
                                        {g}
                                    </Badge>
                                ))}
                            </div>
                        </div>

                        <div className="flex gap-3 flex-wrap">
                            <Button className="flex-1 gap-2" size="lg">
                                <Play className="h-4 w-4" />
                                Watch
                            </Button>

                            <Button
                                variant={isBookmarked ? "default" : "outline"}
                                size="icon"
                                type="button"
                                onClick={handleBookmark}
                                className={
                                    isBookmarked
                                        ? "bg-primary text-primary-foreground"
                                        : ""
                                }
                            >
                                {isBookmarked ? (
                                    <BookmarkCheck className="h-5 w-5" />
                                ) : (
                                    <Bookmark className="h-5 w-5" />
                                )}
                            </Button>

                            <Button
                                variant={watched ? "default" : "outline"}
                                size="sm"
                                type="button"
                                onClick={handleMarkWatched}
                                className="flex items-center gap-2"
                            >
                                <Eye className="h-4 w-4" />
                                Mark as watched
                            </Button>

                            <Button
                                variant={tempBookmarked ? "default" : "outline"}
                                size="sm"
                                type="button"
                                onClick={handleTempBookmark}
                                className="flex items-center gap-2"
                            >
                                <Clock className="h-4 w-4" />
                                Temp bookmark
                            </Button>
                        </div>
                    </div>

                    {/* Правая колонка: инфа, каст, факты, рейтинг */}
                    <div className="space-y-6">
                        <div className="space-y-2">
                            <h1 className="text-3xl md:text-4xl font-serif font-bold">
                                {movie.title}
                            </h1>
                            <div className="flex flex-wrap items-center gap-2 text-sm text-muted-foreground">
                                <span>{movie.productionYear}</span>
                                {movie.duration && (
                                    <>
                                        <span>•</span>
                                        <span>{movie.duration}</span>
                                    </>
                                )}
                                {movie.country && (
                                    <>
                                        <span>•</span>
                                        <span>{movie.country}</span>
                                    </>
                                )}
                                {movie.language && (
                                    <>
                                        <span>•</span>
                                        <span>{movie.language}</span>
                                    </>
                                )}
                            </div>
                            {movie.quote && (
                                <p className="italic text-muted-foreground mt-2">
                                    “{movie.quote}”
                                </p>
                            )}
                        </div>

                        <div className="space-y-2">
                            <h2 className="text-lg font-semibold">Overview</h2>
                            {movie.description ? (
                                <p className="text-sm md:text-base text-muted-foreground whitespace-pre-line">
                                    {movie.description}
                                </p>
                            ) : (
                                <p className="text-sm text-muted-foreground">
                                    No description provided.
                                </p>
                            )}
                        </div>

                        <div className="grid gap-4 sm:grid-cols-2">
                            {directors.length > 0 ? (
                                <div>
                                    <p className="text-xs uppercase text-muted-foreground">
                                        Director
                                    </p>
                                    <p className="text-sm">
                                        {directors.map((d) => d.fullName).join(", ")}
                                    </p>
                                </div>
                            ) : (
                                movie.director && (
                                    <div>
                                        <p className="text-xs uppercase text-muted-foreground">
                                            Director
                                        </p>
                                        <p className="text-sm">{movie.director}</p>
                                    </div>
                                )
                            )}

                            {movie.productionCompany && (
                                <div>
                                    <p className="text-xs uppercase text-muted-foreground">
                                        Production
                                    </p>
                                    <p className="text-sm">
                                        {movie.productionCompany}
                                    </p>
                                </div>
                            )}
                            {movie.certificate && (
                                <div>
                                    <p className="text-xs uppercase text-muted-foreground">
                                        Certificate
                                    </p>
                                    <p className="text-sm">{movie.certificate}</p>
                                </div>
                            )}
                            {(movie.budget || movie.grossWorldwide) && (
                                <div>
                                    <p className="text-xs uppercase text-muted-foreground">
                                        Box office
                                    </p>
                                    <p className="text-sm">
                                        {movie.budget && (
                                            <>
                                                Budget: {movie.budget}
                                                <br />
                                            </>
                                        )}
                                        {movie.grossWorldwide && (
                                            <>Worldwide: {movie.grossWorldwide}</>
                                        )}
                                    </p>
                                </div>
                            )}
                        </div>

                        {/* Genres */}
                        {movie.genres.length > 0 && (
                            <div className="space-y-2">
                                <h2 className="text-lg font-semibold">Genres</h2>
                                <div className="flex flex-wrap gap-2">
                                    {movie.genres.map((g) => (
                                        <Badge key={g} variant="outline">
                                            {g}
                                        </Badge>
                                    ))}
                                </div>
                            </div>
                        )}

                        {/* Top cast */}
                        {topCast.length > 0 && (
                            <div className="space-y-3">
                                <div className="flex items-center justify-between">
                                    <h2 className="text-lg font-semibold">Top cast</h2>
                                    {movie.credits.length > 0 && (
                                        <Button
                                            type="button"
                                            variant="ghost"
                                            size="sm"
                                            onClick={() =>
                                                router.push(`/cinema/${movie.id}/cast`)
                                            }
                                        >
                                            Show all cast
                                        </Button>
                                    )}
                                </div>
                                <div className="space-y-2">
                                    {topCast.map((c) => (
                                        <div
                                            key={c.id || `${c.fullName}-${c.order}`}
                                            className="flex items-center gap-3 border border-border rounded-md px-3 py-2 bg-card/30"
                                        >
                                            <div className="w-10 h-10 rounded-full bg-muted overflow-hidden flex items-center justify-center text-xs flex-shrink-0">
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
                                            <div className="flex-1">
                                                <p className="text-sm font-semibold">
                                                    {c.fullName}
                                                    <span className="ml-2 text-xs text-muted-foreground">
                                                        {c.role}
                                                    </span>
                                                </p>
                                                {c.characterName && (
                                                    <p className="text-xs text-muted-foreground">
                                                        as{" "}
                                                        <span className="italic">
                                                            {c.characterName}
                                                        </span>
                                                    </p>
                                                )}
                                            </div>
                                            {c.order > 0 && (
                                                <span className="text-[11px] text-muted-foreground">
                                                    #{c.order}
                                                </span>
                                            )}
                                        </div>
                                    ))}
                                </div>
                            </div>
                        )}

                        {/* Cards */}
                        {movie.cards.length > 0 && (
                            <div className="space-y-3">
                                <h2 className="text-lg font-semibold">Cards</h2>
                                <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                                    {movie.cards.map((c, i) => (
                                        <Card
                                            key={i}
                                            className="overflow-hidden bg-card/40 border-border"
                                        >
                                            {c.imageUrl && (
                                                <div className="aspect-video bg-muted overflow-hidden">
                                                    <img
                                                        src={`/api/gw${c.imageUrl}`}
                                                        alt={c.title || movie.title}
                                                        className="w-full h-full object-cover"
                                                    />
                                                </div>
                                            )}
                                            <CardContent className="p-3 space-y-1">
                                                {c.title && (
                                                    <p className="text-sm font-semibold">
                                                        {c.title}
                                                    </p>
                                                )}
                                                {c.description && (
                                                    <p className="text-xs text-muted-foreground whitespace-pre-line">
                                                        {c.description}
                                                    </p>
                                                )}
                                            </CardContent>
                                        </Card>
                                    ))}
                                </div>
                            </div>
                        )}

                        {/* Facts */}
                        {movie.facts.length > 0 && (
                            <div className="space-y-3">
                                <h2 className="text-lg font-semibold">
                                    Interesting facts
                                </h2>
                                <div className="space-y-3">
                                    {movie.facts.map((f, i) => (
                                        <div
                                            key={i}
                                            className="border border-border rounded-md p-3 space-y-1 bg-card/30"
                                        >
                                            <p className="text-sm font-semibold">
                                                {f.factName}
                                            </p>
                                            {f.text && (
                                                <p className="text-xs text-muted-foreground whitespace-pre-line">
                                                    {f.text}
                                                </p>
                                            )}
                                        </div>
                                    ))}
                                </div>
                            </div>
                        )}

                        {/* Rating section */}
                        <div className="space-y-2">
                            <h2 className="text-lg font-semibold">Rating</h2>
                            {ratingLoading && (
                                <p className="text-sm text-muted-foreground">
                                    Loading rating…
                                </p>
                            )}
                            {ratingError && (
                                <p className="text-sm text-destructive">
                                    {ratingError}
                                </p>
                            )}

                            {ratingSummary && (
                                <div className="flex flex-col gap-2 sm:flex-row sm:items-center sm:justify-between">
                                    <div className="space-y-1">
                                        <div className="flex items-baseline gap-2">
                                            <span className="text-2xl font-bold">
                                                {ratingSummary.averageRating !== null
                                                    ? ratingSummary.averageRating.toFixed(1)
                                                    : "—"}
                                            </span>
                                            <span className="text-xs uppercase text-muted-foreground">
                                                global
                                            </span>
                                        </div>
                                        <p className="text-xs text-muted-foreground">
                                            {ratingSummary.ratingsCount}{" "}
                                            {ratingSummary.ratingsCount === 1
                                                ? "vote"
                                                : "votes"}
                                        </p>
                                    </div>

                                    <div className="flex flex-col items-start gap-1">
                                        <div className="flex items-center gap-2">
                                            <RatingStars
                                                max={10}
                                                value={ratingSummary.userRating ?? null}
                                                onChange={handleSetRating}
                                                size="sm"
                                            />
                                            <span className="text-xs text-muted-foreground">
                                                Your rating:{" "}
                                                {ratingSummary.userRating
                                                    ? `${ratingSummary.userRating}/10`
                                                    : "—"}
                                            </span>
                                        </div>

                                        {ratingSummary.userRating && (
                                            <button
                                                type="button"
                                                onClick={handleDeleteRating}
                                                className="text-xs text-muted-foreground underline-offset-2 hover:underline"
                                                disabled={ratingLoading}
                                            >
                                                Remove rating &amp; clear history
                                            </button>
                                        )}
                                    </div>
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            </main>
        </div>
    );
}
