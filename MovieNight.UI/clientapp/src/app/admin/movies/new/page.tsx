"use client";

import { useState, FormEvent } from "react";
import { useRouter } from "next/navigation";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { useCurrentProfile } from "@/lib/use-current-profile";
import { ArrowLeft, Film, Upload, X, Plus } from "lucide-react";
import { TopBar } from "@/components/top-bar";
import {
    PersonPickerDialog,
    SelectedPerson,
} from "@/components/person-picker-dialog";

type MediaUploadResponse = {
    id: string;
    fileName: string;
    contentType: string;
    length: number;
};

type CreditRole = "Actor" | "Director" | "Writer" | "Producer" | "Cameo" | "Other";

type CreditFormRow = {
    personId?: string;
    fullName: string;
    role: CreditRole;
    characterName: string;
    order: string; 
    profileImagePath?: string;
    knownForDepartment?: string | null;
};

type CategoryKey = "Non" | "Film" | "Serial" | "Cartoon" | "Anime";

const categoryMap: Record<CategoryKey, number> = {
    Non: 0,
    Film: 1,
    Serial: 2,
    Cartoon: 3,
    Anime: 4,
};

const creditRoleNameToNumber: Record<CreditRole, number> = {
    Actor: 1,
    Director: 2,
    Writer: 3,
    Producer: 4,
    Cameo: 5,
    Other: 99,
};

type CardFormRow = {
    title: string;
    description: string;
    file: File | null;
};

type FactFormRow = {
    factName: string;
    text: string;
};

type NormalizedCredit = {
    personId: string;
    fullName: string;
    roleName: CreditRole;
    roleValue: number;
    characterName: string | null;
    order: number | null;
};

const mapDepartmentToRole = (dept?: string | null): CreditRole => {
    if (!dept) return "Actor";
    const d = dept.toLowerCase();
    if (d.includes("direct")) return "Director";
    if (d.includes("writ")) return "Writer";
    if (d.includes("produc")) return "Producer";
    if (d.includes("act")) return "Actor";
    return "Other";
};

export default function NewMoviePage() {
    const router = useRouter();
    const { hasAdminAccess, loading } = useCurrentProfile();

    const [credits, setCredits] = useState<CreditFormRow[]>([]);
    const [title, setTitle] = useState("");
    const [year, setYear] = useState("");
    const [director, setDirector] = useState("");
    const [description, setDescription] = useState("");
    const [quote, setQuote] = useState("");
    const [country, setCountry] = useState("Other");
    const [language, setLanguage] = useState("English");
    const [duration, setDuration] = useState("");
    const [posterFile, setPosterFile] = useState<File | null>(null);
    const [category, setCategory] = useState<CategoryKey>("Film");

    // genres
    const [genres, setGenres] = useState<string[]>([]);
    const [newGenre, setNewGenre] = useState("");

    // cards
    const [cards, setCards] = useState<CardFormRow[]>([]);

    // facts
    const [facts, setFacts] = useState<FactFormRow[]>([]);

    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // === Person dialog ===
    const [personDialogOpen, setPersonDialogOpen] = useState(false);
    const [editCreditIndex, setEditCreditIndex] = useState<number | null>(null);
    const [personDialogInitialName, setPersonDialogInitialName] = useState("");
    const [personDialogDepartment, setPersonDialogDepartment] =
        useState<string>("Other");

    // ===== Genres helpers =====
    const addGenre = () => {
        const g = newGenre.trim();
        if (!g) return;
        if (!genres.includes(g)) {
            setGenres((prev) => [...prev, g]);
        }
        setNewGenre("");
    };

    const removeGenre = (index: number) => {
        setGenres((prev) => prev.filter((_, i) => i !== index));
    };

    // ===== Cards helpers =====
    const addCard = () => {
        setCards((prev) => [...prev, { title: "", description: "", file: null }]);
    };

    const setCardTitle = (index: number, value: string) => {
        setCards((prev) =>
            prev.map((c, i) => (i === index ? { ...c, title: value } : c)),
        );
    };

    const setCardDescription = (index: number, value: string) => {
        setCards((prev) =>
            prev.map((c, i) => (i === index ? { ...c, description: value } : c)),
        );
    };

    const setCardFile = (index: number, file: File | null) => {
        setCards((prev) =>
            prev.map((c, i) => (i === index ? { ...c, file } : c)),
        );
    };

    const removeCard = (index: number) => {
        setCards((prev) => prev.filter((_, i) => i !== index));
    };

    // ===== Facts helpers =====
    const addFact = () => {
        setFacts((prev) => [...prev, { factName: "", text: "" }]);
    };

    const setFactName = (index: number, value: string) => {
        setFacts((prev) =>
            prev.map((f, i) => (i === index ? { ...f, factName: value } : f)),
        );
    };

    const setFactText = (index: number, value: string) => {
        setFacts((prev) =>
            prev.map((f, i) => (i === index ? { ...f, text: value } : f)),
        );
    };

    const removeFact = (index: number) => {
        setFacts((prev) => prev.filter((_, i) => i !== index));
    };

    // ===== Credits helpers =====

    const setCreditField = (
        index: number,
        field: keyof CreditFormRow,
        value: any,
    ) => {
        setCredits((prev) =>
            prev.map((row, i) =>
                i === index ? { ...row, [field]: value } : row,
            ),
        );
    };

    const handleAddCreditClick = () => {
        setEditCreditIndex(null);
        setPersonDialogInitialName("");
        setPersonDialogDepartment("Other");
        setPersonDialogOpen(true);
    };

    const openPersonDialogForRow = (index: number) => {
        const row = credits[index];
        setEditCreditIndex(index);
        setPersonDialogInitialName(row.fullName);
        setPersonDialogDepartment(row.knownForDepartment ?? "Other");
        setPersonDialogOpen(true);
    };

    const removeCredit = (index: number) => {
        setCredits((prev) => prev.filter((_, i) => i !== index));
    };

    const handlePersonChosen = (person: SelectedPerson) => {
        const profileImagePath = person.profileImageId
            ? `/media/${person.profileImageId}`
            : undefined;

        const defaultRole = mapDepartmentToRole(person.knownForDepartment);

        setCredits((prev) => {
            // Add
            if (editCreditIndex === null) {
                const newRow: CreditFormRow = {
                    personId: person.id,
                    fullName: person.fullName,
                    role: defaultRole,
                    characterName: "",
                    order: "",
                    profileImagePath,
                    knownForDepartment: person.knownForDepartment ?? null,
                };
                return [...prev, newRow];
            }

            // Edit
            return prev.map((row, i) =>
                i === editCreditIndex
                    ? {
                        ...row,
                        personId: person.id,
                        fullName: person.fullName,
                        profileImagePath: profileImagePath ?? row.profileImagePath,
                        knownForDepartment: person.knownForDepartment ?? row.knownForDepartment ?? null,
                    }
                    : row,
            );
        });

        setPersonDialogOpen(false);
        setEditCreditIndex(null);
    };

    // ===== upload helper =====
    const uploadMedia = async (file: File): Promise<string> => {
        const formData = new FormData();
        formData.append("file", file);

        const uploadResp = await fetch("/api/gw/media", {
            method: "POST",
            body: formData,
            credentials: "include",
        });

        if (!uploadResp.ok) {
            const txt = await uploadResp.text();
            throw new Error(txt || "Failed to upload file to media service");
        }

        const media = (await uploadResp.json()) as MediaUploadResponse;
        return `/media/${media.id}`;
    };

    // ===== SUBMIT =====
    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        if (!hasAdminAccess) {
            setError("You don't have access to create movies.");
            return;
        }

        if (!title || !year || !director) {
            setError("Title, year and director are required.");
            return;
        }

        const invalidCredits = credits.filter(
            (c) => c.fullName.trim() && !c.personId,
        );
        if (invalidCredits.length > 0) {
            setError(
                "All cast members must be linked to a person. Click each row and select or create a person.",
            );
            return;
        }

        try {
            setSubmitting(true);
            setError(null);

            // 1. poster
            let posterImage = "";
            if (posterFile) {
                posterImage = await uploadMedia(posterFile);
            }

            // 2. cards
            const cardsPayload: {
                title: string;
                imageUrl: string;
                description: string;
            }[] = [];

            if (posterImage && cards.length === 0) {
                cardsPayload.push({
                    title,
                    imageUrl: posterImage,
                    description,
                });
            }

            for (const card of cards) {
                let imageUrl = posterImage;
                if (card.file) {
                    imageUrl = await uploadMedia(card.file);
                }

                cardsPayload.push({
                    title: card.title || title,
                    imageUrl,
                    description: card.description,
                });
            }

            // 3. facts
            const factsPayload = facts
                .filter((f) => f.factName.trim() || f.text.trim())
                .map((f) => ({
                    factName: f.factName.trim() || title,
                    text: f.text.trim(),
                }));

            // 4. normalized credits
            const normalizedCredits: NormalizedCredit[] = credits
                .filter((c) => c.personId && c.fullName.trim())
                .map((c) => ({
                    personId: c.personId as string,
                    fullName: c.fullName.trim(),
                    roleName: c.role,
                    roleValue: creditRoleNameToNumber[c.role],
                    characterName: c.characterName.trim() || null,
                    order: c.order ? Number(c.order) : null,
                }));

            const creditsPayloadForMovie = normalizedCredits.map((c) => ({
                fullName: c.fullName,
                role: c.roleValue,
                characterName: c.characterName,
                order: c.order,
            }));

            const moviePayload = {
                title,
                category: categoryMap[category],
                posterImage,
                quote,
                description,
                productionYear: Number(year),
                country,
                director,
                duration,
                certificate: "",
                productionCompany: "",
                budget: "",
                grossWorldwide: "",
                language,
                genre: genres,
                cards: cardsPayload,
                facts: factsPayload,
                credits: creditsPayloadForMovie,
            };

            const resp = await fetch("/api/gw/movies", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                credentials: "include",
                body: JSON.stringify(moviePayload),
            });

            if (!resp.ok) {
                const txt = await resp.text();
                throw new Error(txt || "Failed to create movie");
            }

            const createdMovie = await resp.json();
            const movieId: string | undefined =
                createdMovie.id ?? createdMovie.movieId;
            if (!movieId) {
                throw new Error("Movie id is missing in create response");
            }

            for (const c of normalizedCredits) {
                try {
                    const creditResp = await fetch("/api/gw/people/credits", {
                        method: "POST",
                        headers: { "Content-Type": "application/json" },
                        credentials: "include",
                        body: JSON.stringify({
                            movieId,
                            personId: c.personId,
                            role: c.roleValue,
                            characterName: c.characterName,
                            order: c.order,
                        }),
                    });

                    if (!creditResp.ok) {
                        console.error(
                            "Failed to create credit for person",
                            c.fullName,
                            await creditResp.text(),
                        );
                    }
                } catch (innerErr) {
                    console.error(
                        "Error while creating credit",
                        c.fullName,
                        innerErr,
                    );
                }
            }

            router.push("/admin");
        } catch (err: any) {
            setError(err.message ?? "Unknown error");
        } finally {
            setSubmitting(false);
        }
    };

    // --- LOADING / NO ACCESS / FORM ---

    if (loading) {
        return (
            <div className="min-h-screen">
                <TopBar />
                <main className="container mx-auto px-4 pt-24 pb-12">
                    <h1 className="text-4xl md:text-5xl font-serif font-bold mb-8">
                        Add movie
                    </h1>
                    <Card className="bg-card/50 backdrop-blur border-border">
                        <CardContent className="p-8">
                            <p className="text-muted-foreground">Loading…</p>
                        </CardContent>
                    </Card>
                </main>
            </div>
        );
    }

    if (!hasAdminAccess) {
        return (
            <div className="min-h-screen">
                <TopBar />
                <main className="container mx-auto px-4 pt-24 pb-12">
                    <h1 className="text-4xl md:text-5xl font-serif font-bold mb-8">
                        Add movie
                    </h1>
                    <Card className="bg-card/50 backdrop-blur border-border">
                        <CardContent className="p-8 text-center">
                            <p className="text-muted-foreground">
                                You don&apos;t have access to this page.
                            </p>
                            <Button
                                variant="outline"
                                className="mt-4 gap-2"
                                onClick={() => router.push("/")}
                            >
                                <ArrowLeft className="h-4 w-4" />
                                Back to home
                            </Button>
                        </CardContent>
                    </Card>
                </main>
            </div>
        );
    }

    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12">
                <div className="max-w-4xl mx-auto">
                    <div className="flex items-center justify-between mb-8">
                        <h1 className="text-4xl md:text-5xl font-serif font-bold">
                            Add movie
                        </h1>
                        <Button
                            variant="ghost"
                            className="gap-2"
                            type="button"
                            onClick={() => router.back()}
                        >
                            <ArrowLeft className="h-4 w-4" />
                            Back
                        </Button>
                    </div>

                    <Card className="bg-card/50 backdrop-blur border-border">
                        <CardHeader className="flex flex-row items-center gap-3 pb-4">
                            <Film className="h-6 w-6 text-primary" />
                            <div>
                                <h2 className="font-semibold">Movie details</h2>
                                <p className="text-sm text-muted-foreground">
                                    Fill in the basic information, upload a poster and
                                    configure genres/cards/facts/cast.
                                </p>
                            </div>
                        </CardHeader>
                        <CardContent className="space-y-6">
                            <form onSubmit={handleSubmit} className="space-y-6">
                                {/* базовые поля */}
                                <div className="grid gap-4 md:grid-cols-2">
                                    <div className="space-y-2">
                                        <Label htmlFor="title">Title</Label>
                                        <Input
                                            id="title"
                                            value={title}
                                            onChange={(e) =>
                                                setTitle(e.target.value)
                                            }
                                            required
                                        />
                                    </div>

                                    <div className="space-y-2">
                                        <Label htmlFor="year">Year</Label>
                                        <Input
                                            id="year"
                                            type="number"
                                            min={1900}
                                            max={2100}
                                            value={year}
                                            onChange={(e) =>
                                                setYear(e.target.value)
                                            }
                                            required
                                        />
                                    </div>
                                </div>

                                <div className="grid gap-4 md:grid-cols-2">
                                    <div className="space-y-2">
                                        <Label htmlFor="director">Director</Label>
                                        <Input
                                            id="director"
                                            value={director}
                                            onChange={(e) =>
                                                setDirector(e.target.value)
                                            }
                                            required
                                        />
                                    </div>

                                    <div className="space-y-2">
                                        <Label htmlFor="category">Category</Label>
                                        <select
                                            id="category"
                                            className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                                            value={category}
                                            onChange={(e) =>
                                                setCategory(
                                                    e.target
                                                        .value as CategoryKey,
                                                )
                                            }
                                        >
                                            <option value="Non">Non</option>
                                            <option value="Film">Film</option>
                                            <option value="Serial">Serial</option>
                                            <option value="Cartoon">
                                                Cartoon
                                            </option>
                                            <option value="Anime">Anime</option>
                                        </select>
                                    </div>
                                </div>

                                <div className="grid gap-4 md:grid-cols-2">
                                    <div className="space-y-2">
                                        <Label htmlFor="country">Country</Label>
                                        <Input
                                            id="country"
                                            value={country}
                                            onChange={(e) =>
                                                setCountry(e.target.value)
                                            }
                                        />
                                    </div>

                                    <div className="space-y-2">
                                        <Label htmlFor="language">Language</Label>
                                        <Input
                                            id="language"
                                            value={language}
                                            onChange={(e) =>
                                                setLanguage(e.target.value)
                                            }
                                        />
                                    </div>
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor="duration">Duration</Label>
                                    <Input
                                        id="duration"
                                        value={duration}
                                        onChange={(e) =>
                                            setDuration(e.target.value)
                                        }
                                        placeholder="e.g. 2h 10m"
                                    />
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor="quote">Quote</Label>
                                    <Input
                                        id="quote"
                                        value={quote}
                                        onChange={(e) =>
                                            setQuote(e.target.value)
                                        }
                                    />
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor="description">
                                        Description
                                    </Label>
                                    <Textarea
                                        id="description"
                                        value={description}
                                        onChange={(e) =>
                                            setDescription(e.target.value)
                                        }
                                        rows={4}
                                    />
                                </div>

                                <div className="space-y-2">
                                    <Label htmlFor="poster">Poster</Label>
                                    <Input
                                        id="poster"
                                        type="file"
                                        accept="image/*"
                                        onChange={(e) =>
                                            setPosterFile(
                                                e.target.files?.[0] ?? null,
                                            )
                                        }
                                    />
                                    <p className="text-xs text-muted-foreground">
                                        Optional, but recommended. The file will be
                                        uploaded to the Media service.
                                    </p>
                                </div>

                                {/* GENRES */}
                                <div className="space-y-2">
                                    <div className="flex items-center justify-between">
                                        <Label>Genres</Label>
                                    </div>
                                    <div className="flex gap-2">
                                        <Input
                                            value={newGenre}
                                            onChange={(e) =>
                                                setNewGenre(e.target.value)
                                            }
                                            placeholder="Type genre and press Add"
                                        />
                                        <Button
                                            type="button"
                                            variant="outline"
                                            onClick={addGenre}
                                        >
                                            <Plus className="h-4 w-4 mr-1" />
                                            Add
                                        </Button>
                                    </div>
                                    {genres.length > 0 && (
                                        <div className="flex flex-wrap gap-2 mt-2">
                                            {genres.map((g, i) => (
                                                <span
                                                    key={i}
                                                    className="inline-flex items-center gap-1 rounded-full border px-3 py-1 text-xs"
                                                >
                                                    {g}
                                                    <button
                                                        type="button"
                                                        onClick={() =>
                                                            removeGenre(i)
                                                        }
                                                        className="text-muted-foreground hover:text-destructive"
                                                    >
                                                        <X className="h-3 w-3" />
                                                    </button>
                                                </span>
                                            ))}
                                        </div>
                                    )}
                                    {genres.length === 0 && (
                                        <p className="text-xs text-muted-foreground">
                                            No genres yet. Add at least one if you
                                            want better filtering.
                                        </p>
                                    )}
                                </div>

                                {/* CREDITS: Cast & Crew */}
                                <div className="space-y-2">
                                    <div className="flex items-center justify-between">
                                        <Label>Cast &amp; Crew</Label>
                                        <Button
                                            type="button"
                                            variant="outline"
                                            size="sm"
                                            onClick={handleAddCreditClick}
                                            className="gap-1"
                                        >
                                            <Plus className="h-4 w-4" />
                                            Add person
                                        </Button>
                                    </div>

                                    {credits.length === 0 && (
                                        <p className="text-xs text-muted-foreground">
                                            Optional. Click &quot;Add person&quot;
                                            to search or create people in the People
                                            service and link them as cast for this
                                            movie.
                                        </p>
                                    )}

                                    {credits.length > 0 && (
                                        <div className="space-y-3">
                                            {credits.map((c, i) => (
                                                <div
                                                    key={i}
                                                    className="border rounded-md p-3 space-y-2 bg-card/30"
                                                >
                                                    <div className="flex items-center justify-between gap-3">
                                                        <button
                                                            type="button"
                                                            className="flex items-center gap-3 flex-1 text-left"
                                                            onClick={() =>
                                                                openPersonDialogForRow(
                                                                    i,
                                                                )
                                                            }
                                                        >
                                                            <div className="w-9 h-9 rounded-full bg-muted overflow-hidden flex items-center justify-center text-[11px] flex-shrink-0">
                                                                {c.profileImagePath ? (
                                                                    <img
                                                                        src={`/api/gw${c.profileImagePath}`}
                                                                        alt={c.fullName}
                                                                        className="w-full h-full object-cover"
                                                                    />
                                                                ) : (
                                                                    <span className="text-muted-foreground">
                                                                        {c.fullName
                                                                            .split(
                                                                                " ",
                                                                            )
                                                                            .map(
                                                                                (x) =>
                                                                                    x[0],
                                                                            )
                                                                            .join(
                                                                                "",
                                                                            )
                                                                            .slice(
                                                                                0,
                                                                                2,
                                                                            )
                                                                            .toUpperCase()}
                                                                    </span>
                                                                )}
                                                            </div>
                                                            <div>
                                                                <p className="text-sm font-semibold">
                                                                    {c.fullName ||
                                                                        "(No name)"}
                                                                </p>
                                                                {c.knownForDepartment && (
                                                                    <p className="text-xs text-muted-foreground">
                                                                        {c.knownForDepartment}
                                                                    </p>
                                                                )}
                                                                <p className="text-[11px] text-muted-foreground">
                                                                    Click to change person
                                                                </p>
                                                            </div>
                                                        </button>

                                                        <Button
                                                            type="button"
                                                            variant="ghost"
                                                            size="icon"
                                                            onClick={() =>
                                                                removeCredit(i)
                                                            }
                                                        >
                                                            <X className="h-4 w-4" />
                                                        </Button>
                                                    </div>

                                                    <div className="grid gap-3 md:grid-cols-3">
                                                        <div className="space-y-1">
                                                            <Label>Role</Label>
                                                            <select
                                                                className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                                                                value={c.role}
                                                                onChange={(e) =>
                                                                    setCreditField(
                                                                        i,
                                                                        "role",
                                                                        e.target
                                                                            .value as CreditRole,
                                                                    )
                                                                }
                                                            >
                                                                <option value="Actor">
                                                                    Actor
                                                                </option>
                                                                <option value="Director">
                                                                    Director
                                                                </option>
                                                                <option value="Writer">
                                                                    Writer
                                                                </option>
                                                                <option value="Producer">
                                                                    Producer
                                                                </option>
                                                                <option value="Cameo">
                                                                    Cameo
                                                                </option>
                                                                <option value="Other">
                                                                    Other
                                                                </option>
                                                            </select>
                                                        </div>

                                                        <div className="space-y-1">
                                                            <Label>
                                                                Character name
                                                            </Label>
                                                            <Input
                                                                value={
                                                                    c.characterName
                                                                }
                                                                onChange={(e) =>
                                                                    setCreditField(
                                                                        i,
                                                                        "characterName",
                                                                        e.target
                                                                            .value,
                                                                    )
                                                                }
                                                                placeholder="Neo"
                                                            />
                                                        </div>

                                                        <div className="space-y-1">
                                                            <Label>Order</Label>
                                                            <Input
                                                                type="number"
                                                                min={1}
                                                                value={c.order}
                                                                onChange={(e) =>
                                                                    setCreditField(
                                                                        i,
                                                                        "order",
                                                                        e.target
                                                                            .value,
                                                                    )
                                                                }
                                                                placeholder="1"
                                                            />
                                                        </div>
                                                    </div>
                                                </div>
                                            ))}
                                        </div>
                                    )}

                                    <PersonPickerDialog
                                        open={personDialogOpen}
                                        onOpenChange={(open) => {
                                            if (!open) {
                                                setEditCreditIndex(null);
                                            }
                                            setPersonDialogOpen(open);
                                        }}
                                        initialName={personDialogInitialName}
                                        defaultDepartment={personDialogDepartment}
                                        onPersonChosen={handlePersonChosen}
                                    />
                                </div>

                                {/* CARDS */}
                                <div className="space-y-2">
                                    <div className="flex items-center justify-between">
                                        <Label>Cards</Label>
                                        <Button
                                            type="button"
                                            variant="outline"
                                            size="sm"
                                            onClick={addCard}
                                        >
                                            <Plus className="h-4 w-4 mr-1" />
                                            Add card
                                        </Button>
                                    </div>
                                    {cards.length === 0 && (
                                        <p className="text-xs text-muted-foreground">
                                            Cards are optional. If you don&apos;t add
                                            any, a single card based on the poster and
                                            description will be created.
                                        </p>
                                    )}
                                    <div className="space-y-4">
                                        {cards.map((card, i) => (
                                            <div
                                                key={i}
                                                className="border rounded-md p-3 space-y-3"
                                            >
                                                <div className="flex items-center justify-between">
                                                    <span className="text-xs text-muted-foreground">
                                                        Card #{i + 1}
                                                    </span>
                                                    <Button
                                                        type="button"
                                                        variant="ghost"
                                                        size="sm"
                                                        onClick={() =>
                                                            removeCard(i)
                                                        }
                                                    >
                                                        <X className="h-4 w-4 mr-1" />
                                                        Remove
                                                    </Button>
                                                </div>
                                                <div className="grid gap-3 md:grid-cols-2">
                                                    <div className="space-y-1">
                                                        <Label>Title</Label>
                                                        <Input
                                                            value={card.title}
                                                            onChange={(e) =>
                                                                setCardTitle(
                                                                    i,
                                                                    e.target.value,
                                                                )
                                                            }
                                                        />
                                                    </div>
                                                    <div className="space-y-1">
                                                        <Label>Image</Label>
                                                        <Input
                                                            type="file"
                                                            accept="image/*"
                                                            onChange={(e) =>
                                                                setCardFile(
                                                                    i,
                                                                    e.target
                                                                        .files?.[0] ??
                                                                    null,
                                                                )
                                                            }
                                                        />
                                                        <p className="text-[10px] text-muted-foreground">
                                                            If empty, poster image
                                                            will be used.
                                                        </p>
                                                    </div>
                                                </div>
                                                <div className="space-y-1">
                                                    <Label>Description</Label>
                                                    <Textarea
                                                        rows={2}
                                                        value={card.description}
                                                        onChange={(e) =>
                                                            setCardDescription(
                                                                i,
                                                                e.target.value,
                                                            )
                                                        }
                                                    />
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                </div>

                                {/* FACTS */}
                                <div className="space-y-2">
                                    <div className="flex items-center justify-between">
                                        <Label>Interesting facts</Label>
                                        <Button
                                            type="button"
                                            variant="outline"
                                            size="sm"
                                            onClick={addFact}
                                        >
                                            <Plus className="h-4 w-4 mr-1" />
                                            Add fact
                                        </Button>
                                    </div>
                                    {facts.length === 0 && (
                                        <p className="text-xs text-muted-foreground">
                                            Facts are optional. You can add trivia,
                                            behind-the-scenes info, etc.
                                        </p>
                                    )}
                                    <div className="space-y-4">
                                        {facts.map((fact, i) => (
                                            <div
                                                key={i}
                                                className="border rounded-md p-3 space-y-3 bg-card/30"
                                            >
                                                <div className="space-y-1">
                                                    <Label>Fact title</Label>
                                                    <Input
                                                        value={fact.factName}
                                                        onChange={(e) =>
                                                            setFactName(
                                                                i,
                                                                e.target.value,
                                                            )
                                                        }
                                                    />
                                                </div>
                                                <div className="space-y-1">
                                                    <Label>Text</Label>
                                                    <Textarea
                                                        rows={2}
                                                        value={fact.text}
                                                        onChange={(e) =>
                                                            setFactText(
                                                                i,
                                                                e.target.value,
                                                            )
                                                        }
                                                    />
                                                </div>
                                                <div className="text-right">
                                                    <Button
                                                        type="button"
                                                        variant="ghost"
                                                        size="sm"
                                                        onClick={() =>
                                                            removeFact(i)
                                                        }
                                                    >
                                                        <X className="h-4 w-4 mr-1" />
                                                        Remove
                                                    </Button>
                                                </div>
                                            </div>
                                        ))}
                                    </div>
                                </div>

                                {error && (
                                    <p className="text-sm text-destructive">
                                        {error}
                                    </p>
                                )}

                                <div className="flex justify-end gap-3">
                                    <Button
                                        type="button"
                                        variant="outline"
                                        onClick={() => router.back()}
                                        className="gap-2"
                                    >
                                        <ArrowLeft className="h-4 w-4" />
                                        Cancel
                                    </Button>
                                    <Button
                                        type="submit"
                                        disabled={submitting}
                                        className="gap-2"
                                    >
                                        {submitting && (
                                            <Upload className="h-4 w-4 animate-spin" />
                                        )}
                                        {submitting ? "Saving…" : "Save movie"}
                                    </Button>
                                </div>
                            </form>
                        </CardContent>
                    </Card>
                </div>
            </main>
        </div>
    );
}
