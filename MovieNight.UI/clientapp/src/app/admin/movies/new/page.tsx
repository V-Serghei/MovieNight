"use client";

import { useState, FormEvent } from "react";
import { useRouter } from "next/navigation";
import { Card, CardHeader, CardContent } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import { Button } from "@/components/ui/button";
import { useCurrentProfile } from "@/lib/use-current-profile";
import { ArrowLeft, Film, Upload } from "lucide-react";

type MediaUploadResponse = {
    id: string;
    fileName: string;
    contentType: string;
    length: number;
};

export default function NewMoviePage() {
    const router = useRouter();
    const { hasAdminAccess, loading } = useCurrentProfile();

    const [title, setTitle] = useState("");
    const [year, setYear] = useState("");
    const [director, setDirector] = useState("");
    const [description, setDescription] = useState("");
    const [quote, setQuote] = useState("");
    const [country, setCountry] = useState("Other");
    const [language, setLanguage] = useState("English");
    const [category, setCategory] = useState<"Film" | "Series" | "Cartoon" | "Anime">("Film");
    const [posterFile, setPosterFile] = useState<File | null>(null);

    const [submitting, setSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

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

        try {
            setSubmitting(true);
            setError(null);

            let posterImage = "";

            if (posterFile) {
                const formData = new FormData();
                formData.append("file", posterFile);

                const uploadResp = await fetch("/api/gw/media", {
                    method: "POST",
                    body: formData,
                    credentials: "include",
                });

                if (!uploadResp.ok) {
                    const txt = await uploadResp.text();
                    throw new Error(
                        txt || "Failed to upload poster to media service"
                    );
                }

                const media = (await uploadResp.json()) as MediaUploadResponse;

                posterImage = `/media/${media.id}`;
            }

            const payload = {
                title,
                category, 
                posterImage,
                quote,
                description,
                productionYear: Number(year),
                country,
                director,
                duration: "",
                certificate: "",
                productionCompany: "",
                budget: "",
                grossWorldwide: "",
                language,
                genre: [] as string[],
            };

            const resp = await fetch("/api/gw/movies", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                credentials: "include",
                body: JSON.stringify(payload),
            });

            if (!resp.ok) {
                const txt = await resp.text();
                throw new Error(txt || "Failed to create movie");
            }

            router.push("/admin");
        } catch (err: any) {
            setError(err.message ?? "Unknown error");
        } finally {
            setSubmitting(false);
        }
    };

    if (loading) {
        return (
            <div className="max-w-4xl mx-auto">
                <h1 className="text-4xl md:text-5xl font-serif font-bold mb-8">
                    Add movie
                </h1>
                <Card className="bg-card/50 backdrop-blur border-border">
                    <CardContent className="p-8">
                        <p className="text-muted-foreground">Loading…</p>
                    </CardContent>
                </Card>
            </div>
        );
    }

    if (!hasAdminAccess) {
        return (
            <div className="max-w-4xl mx-auto">
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
            </div>
        );
    }

    return (
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
                            Fill in the basic information and upload a poster.
                        </p>
                    </div>
                </CardHeader>
                <CardContent className="space-y-6">
                    <form onSubmit={handleSubmit} className="space-y-6">
                        <div className="grid gap-4 md:grid-cols-2">
                            <div className="space-y-2">
                                <Label htmlFor="title">Title</Label>
                                <Input
                                    id="title"
                                    value={title}
                                    onChange={(e) => setTitle(e.target.value)}
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
                                    onChange={(e) => setYear(e.target.value)}
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
                                            e.target.value as
                                                | "Film"
                                                | "Series"
                                                | "Cartoon"
                                                | "Anime"
                                        )
                                    }
                                >
                                    <option value="Film">Film</option>
                                    <option value="Series">Series</option>
                                    <option value="Cartoon">Cartoon</option>
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
                            <Label htmlFor="quote">Quote</Label>
                            <Input
                                id="quote"
                                value={quote}
                                onChange={(e) => setQuote(e.target.value)}
                            />
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="description">Description</Label>
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
                                        e.target.files?.[0] ?? null
                                    )
                                }
                            />
                            <p className="text-xs text-muted-foreground">
                                Optional, but recommended. The file will be
                                uploaded to the Media service.
                            </p>
                        </div>

                        {error && (
                            <p className="text-sm text-destructive">{error}</p>
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
    );
}
