"use client";

import { useEffect, useState } from "react";
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
    DialogDescription,
    DialogFooter,
} from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Button } from "@/components/ui/button";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";

export type SelectedPerson = {
    id: string;
    fullName: string;
    knownForDepartment?: string | null;
    birthDate?: string | null;
    country?: string | null;
    bio?: string | null;
    profileImageId?: string | null;
};

type PersonPickerDialogProps = {
    open: boolean;
    onOpenChange: (open: boolean) => void;
    initialName?: string;
    defaultDepartment?: string;
    onPersonChosen: (person: SelectedPerson) => void;
};

type PersonSearchItem = {
    id: string;
    fullName: string;
    knownForDepartment?: string | null;
    birthDate?: string | null;
    country?: string | null;
    bio?: string | null;
    profileImageId?: string | null;
};

type MediaUploadResponse = {
    id: string;
    fileName: string;
    contentType: string;
    length: number;
};

export function PersonPickerDialog({
                                       open,
                                       onOpenChange,
                                       initialName,
                                       defaultDepartment = "Other",
                                       onPersonChosen,
                                   }: PersonPickerDialogProps) {
    const [tab, setTab] = useState<"search" | "create">("search");

    // --- SEARCH ---
    const [search, setSearch] = useState(initialName ?? "");
    const [searching, setSearching] = useState(false);
    const [results, setResults] = useState<PersonSearchItem[]>([]);
    const [searchError, setSearchError] = useState<string | null>(null);

    // --- CREATE 
    const [fullName, setFullName] = useState(initialName ?? "");
    const [department, setDepartment] = useState(defaultDepartment);
    const [birthDate, setBirthDate] = useState<string>(""); // YYYY-MM-DD
    const [countryValue, setCountryValue] = useState<string>("");
    const [bioValue, setBioValue] = useState<string>("");
    const [profileFile, setProfileFile] = useState<File | null>(null);
    const [creating, setCreating] = useState(false);
    const [createError, setCreateError] = useState<string | null>(null);

    useEffect(() => {
        if (open && initialName) {
            setSearch(initialName);
            setFullName(initialName);
        }
    }, [open, initialName]);

    const resetOnClose = () => {
        setSearchError(null);
        setCreateError(null);
        setSearching(false);
        setCreating(false);
    };

    const handleClose = (value: boolean) => {
        if (!value) {
            resetOnClose();
        }
        onOpenChange(value);
    };

    // ===== SEARCH =====
    const handleSearch = async () => {
        const name = search.trim();
        if (!name) {
            setSearchError("Type at least one character.");
            return;
        }

        try {
            setSearching(true);
            setSearchError(null);
            setResults([]);

            const resp = await fetch(
                `/api/gw/people/search?name=${encodeURIComponent(name)}`,
                { credentials: "include" },
            );

            if (!resp.ok) {
                const txt = await resp.text();
                throw new Error(txt || `Search failed (${resp.status})`);
            }

            const json = await resp.json();

            const mapped: PersonSearchItem[] = Array.isArray(json)
                ? json
                    .map((p: any) => ({
                        id: p.id ?? p.personId,
                        fullName: p.fullName ?? p.personFullName ?? "",
                        knownForDepartment: p.knownForDepartment ?? null,
                        birthDate: p.birthDate ?? null,
                        country: p.country ?? null,
                        bio: p.bio ?? null,
                        profileImageId:
                            p.profileImageId ?? p.personProfileImageId ?? null,
                    }))
                    .filter((p) => p.id && p.fullName)
                : [];

            setResults(mapped);
        } catch (err: any) {
            setSearchError(err.message ?? "Unknown error");
        } finally {
            setSearching(false);
        }
    };

    const uploadProfileImage = async (file: File): Promise<string> => {
        const formData = new FormData();
        formData.append("file", file);

        const resp = await fetch("/api/gw/media", {
            method: "POST",
            body: formData,
            credentials: "include",
        });

        if (!resp.ok) {
            const txt = await resp.text();
            throw new Error(txt || "Failed to upload profile image");
        }

        const media = (await resp.json()) as MediaUploadResponse;
        return media.id; // GUID
    };

    // ===== CREATE PERSON =====
    const handleCreate = async () => {
        const name = fullName.trim();
        if (!name) {
            setCreateError("Full name is required.");
            return;
        }

        try {
            setCreating(true);
            setCreateError(null);

            let profileImageId: string | null = null;
            if (profileFile) {
                profileImageId = await uploadProfileImage(profileFile);
            }

            const resp = await fetch("/api/gw/people", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                credentials: "include",
                body: JSON.stringify({
                    fullName: name,
                    knownForDepartment: department || null,
                    birthDate: birthDate || null,
                    country: countryValue.trim() || null,
                    bio: bioValue.trim() || null,
                    profileImageId: profileImageId,
                }),
            });

            if (!resp.ok) {
                const txt = await resp.text();
                throw new Error(txt || "Failed to create person");
            }

            const json = await resp.json();

            const id: string | undefined = json.id ?? json.personId;
            const createdProfileId: string | null =
                json.profileImageId ?? profileImageId ?? null;

            if (!id) {
                throw new Error("Person id is missing in response");
            }

            const birthDateFromServer: string | null | undefined = json.birthDate;
            const countryFromServer: string | null | undefined = json.country;
            const bioFromServer: string | null | undefined = json.bio;

            onPersonChosen({
                id,
                fullName: json.fullName ?? json.personFullName ?? name,
                knownForDepartment: json.knownForDepartment ?? department ?? null,
                birthDate: birthDateFromServer ?? (birthDate || null),
                country: countryFromServer ?? (countryValue || null),
                bio: bioFromServer ?? (bioValue || null),
                profileImageId: createdProfileId,
            });

            handleClose(false);
        } catch (err: any) {
            setCreateError(err.message ?? "Unknown error");
        } finally {
            setCreating(false);
        }
    };

    // ===== SELECT EXISTING =====
    const handleSelectExisting = (p: PersonSearchItem) => {
        onPersonChosen({
            id: p.id,
            fullName: p.fullName,
            knownForDepartment: p.knownForDepartment ?? null,
            birthDate: p.birthDate ?? null,
            country: p.country ?? null,
            bio: p.bio ?? null,
            profileImageId: p.profileImageId ?? null,
        });
        handleClose(false);
    };

    return (
        <Dialog open={open} onOpenChange={handleClose}>
            <DialogContent className="max-w-lg">
                <DialogHeader>
                    <DialogTitle>Select or create person</DialogTitle>
                    <DialogDescription>
                        Search existing people or create a new one with full details.
                    </DialogDescription>
                </DialogHeader>

                <div className="space-y-6">
                    {/* табы: Search / Create */}
                    <div className="flex gap-2 text-sm">
                        <Button
                            type="button"
                            variant={tab === "search" ? "default" : "outline"}
                            size="sm"
                            onClick={() => setTab("search")}
                        >
                            Search
                        </Button>
                        <Button
                            type="button"
                            variant={tab === "create" ? "default" : "outline"}
                            size="sm"
                            onClick={() => setTab("create")}
                        >
                            Create new
                        </Button>
                    </div>

                    {/* SEARCH TAB */}
                    {tab === "search" && (
                        <div className="space-y-3">
                            <div className="space-y-1">
                                <Label htmlFor="person-search">Name</Label>
                                <div className="flex gap-2">
                                    <Input
                                        id="person-search"
                                        value={search}
                                        onChange={(e) => setSearch(e.target.value)}
                                        placeholder="Type name to search"
                                    />
                                    <Button
                                        type="button"
                                        onClick={handleSearch}
                                        disabled={searching}
                                    >
                                        {searching ? "Searching…" : "Search"}
                                    </Button>
                                </div>
                            </div>

                            {searchError && (
                                <p className="text-xs text-destructive">
                                    {searchError}
                                </p>
                            )}

                            <div className="border rounded-md max-h-64 overflow-y-auto">
                                {results.length === 0 && (
                                    <p className="text-xs text-muted-foreground p-3">
                                        No results yet. Try searching by name.
                                    </p>
                                )}

                                {results.map((p) => {
                                    const imgPath = p.profileImageId
                                        ? `/media/${p.profileImageId}`
                                        : null;

                                    return (
                                        <button
                                            key={p.id}
                                            type="button"
                                            className="w-full flex items-center gap-3 px-3 py-2 text-left hover:bg-muted/70 border-b last:border-b-0"
                                            onClick={() => handleSelectExisting(p)}
                                        >
                                            <div className="w-10 h-10 rounded-full bg-muted overflow-hidden flex items-center justify-center text-xs">
                                                {imgPath ? (
                                                    <img
                                                        src={`/api/gw${imgPath}`}
                                                        alt={p.fullName}
                                                        className="w-full h-full object-cover"
                                                    />
                                                ) : (
                                                    <span className="text-muted-foreground">
                                                        {p.fullName
                                                            .split(" ")
                                                            .map((x) => x[0])
                                                            .join("")
                                                            .slice(0, 2)
                                                            .toUpperCase()}
                                                    </span>
                                                )}
                                            </div>
                                            <div className="flex flex-col">
                                                <span className="text-sm font-medium">
                                                    {p.fullName}
                                                </span>
                                                {p.knownForDepartment && (
                                                    <span className="text-xs text-muted-foreground">
                                                        {p.knownForDepartment}
                                                    </span>
                                                )}
                                                {(p.birthDate || p.country) && (
                                                    <span className="text-[11px] text-muted-foreground">
                                                        {p.birthDate
                                                            ? `Born: ${String(
                                                                p.birthDate,
                                                            ).substring(0, 10)}`
                                                            : null}
                                                        {p.birthDate && p.country
                                                            ? " • "
                                                            : null}
                                                        {p.country
                                                            ? `Country: ${p.country}`
                                                            : null}
                                                    </span>
                                                )}
                                            </div>
                                        </button>
                                    );
                                })}
                            </div>
                        </div>
                    )}

                    {/* CREATE TAB */}
                    {tab === "create" && (
                        <div className="space-y-3">
                            <div className="space-y-1">
                                <Label htmlFor="person-full-name">Full name</Label>
                                <Input
                                    id="person-full-name"
                                    value={fullName}
                                    onChange={(e) => setFullName(e.target.value)}
                                    placeholder="Keanu Reeves"
                                />
                            </div>

                            <div className="space-y-1">
                                <Label htmlFor="person-dept">Department</Label>
                                <select
                                    id="person-dept"
                                    className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                                    value={department}
                                    onChange={(e) => setDepartment(e.target.value)}
                                >
                                    <option value="Acting">Acting</option>
                                    <option value="Directing">Directing</option>
                                    <option value="Writing">Writing</option>
                                    <option value="Production">Production</option>
                                    <option value="Other">Other</option>
                                </select>
                            </div>

                            <div className="grid gap-3 md:grid-cols-2">
                                <div className="space-y-1">
                                    <Label htmlFor="person-birthdate">
                                        Birth date
                                    </Label>
                                    <Input
                                        id="person-birthdate"
                                        type="date"
                                        value={birthDate}
                                        onChange={(e) =>
                                            setBirthDate(e.target.value)
                                        }
                                    />
                                </div>

                                <div className="space-y-1">
                                    <Label htmlFor="person-country">Country</Label>
                                    <Input
                                        id="person-country"
                                        value={countryValue}
                                        onChange={(e) =>
                                            setCountryValue(e.target.value)
                                        }
                                        placeholder="USA, Canada, etc."
                                    />
                                </div>
                            </div>

                            <div className="space-y-1">
                                <Label htmlFor="person-bio">Bio</Label>
                                <Textarea
                                    id="person-bio"
                                    rows={3}
                                    value={bioValue}
                                    onChange={(e) => setBioValue(e.target.value)}
                                    placeholder="Short biography..."
                                />
                            </div>

                            <div className="space-y-1">
                                <Label htmlFor="person-profile-image">
                                    Profile image
                                </Label>
                                <Input
                                    id="person-profile-image"
                                    type="file"
                                    accept="image/*"
                                    onChange={(e) =>
                                        setProfileFile(e.target.files?.[0] ?? null)
                                    }
                                />
                                <p className="text-[10px] text-muted-foreground">
                                    Optional. Uploaded to Media service and linked
                                    to person.
                                </p>
                            </div>

                            {createError && (
                                <p className="text-xs text-destructive">
                                    {createError}
                                </p>
                            )}
                        </div>
                    )}
                </div>

                <DialogFooter className="mt-4">
                    <Button
                        type="button"
                        variant="outline"
                        onClick={() => handleClose(false)}
                    >
                        Cancel
                    </Button>
                    {tab === "create" && (
                        <Button
                            type="button"
                            onClick={handleCreate}
                            disabled={creating}
                        >
                            {creating ? "Creating…" : "Create & select"}
                        </Button>
                    )}
                </DialogFooter>
            </DialogContent>
        </Dialog>
    );
}
