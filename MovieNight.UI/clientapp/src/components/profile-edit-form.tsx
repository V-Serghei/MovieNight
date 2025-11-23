"use client";

import { FormEvent, useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { useCurrentProfile } from "@/lib/use-current-profile";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

type FormState = {
    userName: string;
    firstName: string;
    lastName: string;
    aboutMe: string;
    quote: string;
    phoneNumber: string;
    gender: string;
    dateOfBirth: string; // yyyy-MM-dd
    country: string;
    facebook: string;
    twitter: string;
    instagram: string;
    gitHub: string;
    personalInfoFriendsOnly: boolean;
    showOnlyBasicInfo: boolean;
    hideBrowsingHistory: boolean;
    hideGrades: boolean;
};

export function ProfileEditForm() {
    const { profile, loading } = useCurrentProfile();
    const router = useRouter();
    const [form, setForm] = useState<FormState>({
        userName: "",
        firstName: "",
        lastName: "",
        aboutMe: "",
        quote: "",
        phoneNumber: "",
        gender: "non",
        dateOfBirth: "",
        country: "",
        facebook: "",
        twitter: "",
        instagram: "",
        gitHub: "",
        personalInfoFriendsOnly: false,
        showOnlyBasicInfo: false,
        hideBrowsingHistory: false,
        hideGrades: false,
    });

    const [avatarFile, setAvatarFile] = useState<File | null>(null);
    const [saving, setSaving] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!profile) return;

        setForm({
            userName: profile.userName ?? "",
            firstName: profile.firstName ?? "",
            lastName: profile.lastName ?? "",
            aboutMe: profile.aboutMe ?? "",
            quote: profile.quote ?? "",
            phoneNumber: profile.phoneNumber ?? "",
            gender: profile.gender ?? "non",
            dateOfBirth: profile.dateOfBirth
                ? profile.dateOfBirth.substring(0, 10)
                : "",
            country: profile.country ?? "",
            facebook: profile.facebook ?? "",
            twitter: profile.twitter ?? "",
            instagram: profile.instagram ?? "",
            gitHub: profile.gitHub ?? "",
            personalInfoFriendsOnly: !!profile.personalInfoFriendsOnly,
            showOnlyBasicInfo: !!profile.showOnlyBasicInfo,
            hideBrowsingHistory: !!profile.hideBrowsingHistory,
            hideGrades: !!profile.hideGrades,
        });
    }, [profile]);

    if (loading) {
        return (
            <Card className="bg-card/50 backdrop-blur">
                <CardContent className="p-8 text-center text-muted-foreground">
                    Loading profile…
                </CardContent>
            </Card>
        );
    }

    if (!profile) {
        return (
            <Card className="bg-card/50 backdrop-blur">
                <CardContent className="p-8 text-center text-muted-foreground">
                    You must be logged in to edit profile.
                </CardContent>
            </Card>
        );
    }

    const handleChange =
        (field: keyof FormState) =>
            (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
                setForm((prev) => ({ ...prev, [field]: e.target.value }));
            };

    const handleCheckbox =
        (field: keyof FormState) => (e: React.ChangeEvent<HTMLInputElement>) => {
            setForm((prev) => ({ ...prev, [field]: e.target.checked }));
        };

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();
        setSaving(true);
        setError(null);

        try {
            let avatarMediaId = profile.avatarMediaId ?? null;

            if (avatarFile) {
                const formData = new FormData();
                formData.append("file", avatarFile);

                const uploadRes = await fetch("/api/gw/media", {
                    method: "POST",
                    body: formData,
                    credentials: "include",
                });

                if (!uploadRes.ok) {
                    throw new Error("Failed to upload avatar");
                }

                const uploadJson = await uploadRes.json();
                avatarMediaId =
                    uploadJson.id ?? uploadJson.mediaId ?? avatarMediaId ?? null;
            }

            const body = {
                userName: form.userName || null,
                firstName: form.firstName || null,
                lastName: form.lastName || null,
                aboutMe: form.aboutMe || null,
                quote: form.quote || null,
                phoneNumber: form.phoneNumber || null,
                gender: form.gender || null,
                dateOfBirth: form.dateOfBirth
                    ? new Date(form.dateOfBirth).toISOString()
                    : null,
                country: form.country || null,
                facebook: form.facebook || null,
                twitter: form.twitter || null,
                instagram: form.instagram || null,
                gitHub: form.gitHub || null,
                personalInfoFriendsOnly: form.personalInfoFriendsOnly,
                showOnlyBasicInfo: form.showOnlyBasicInfo,
                hideBrowsingHistory: form.hideBrowsingHistory,
                hideGrades: form.hideGrades,
                avatarMediaId,
            };

            const res = await fetch("/api/gw/users/me/profile", {
                method: "PUT",
                headers: {
                    "Content-Type": "application/json",
                },
                credentials: "include",
                body: JSON.stringify(body),
            });

            if (!res.ok) {
                const txt = await res.text();
                throw new Error(txt || "Failed to save profile");
            }

            router.push("/profile");
            router.refresh?.();
        } catch (err: any) {
            setError(err?.message ?? "Unknown error");
        } finally {
            setSaving(false);
        }
    };

    return (
        <Card className="bg-card/60 backdrop-blur border-border">
            <CardHeader>
                <h1 className="text-2xl md:text-3xl font-serif font-bold">
                    Profile editing
                </h1>
            </CardHeader>
            <CardContent>
                {error && (
                    <div className="mb-4 text-sm text-red-500 border border-red-500/40 rounded-md px-3 py-2">
                        {error}
                    </div>
                )}

                <form className="space-y-8" onSubmit={handleSubmit}>
                    {/* Personal info */}
                    <section className="space-y-4">
                        <h2 className="text-sm font-semibold tracking-wide uppercase text-muted-foreground">
                            Personal info
                        </h2>

                        <div className="grid gap-4 md:grid-cols-2">
                            <div className="space-y-1">
                                <label className="text-xs font-medium uppercase text-muted-foreground">
                                    Username
                                </label>
                                <Input
                                    value={form.userName}
                                    onChange={handleChange("userName")}
                                    placeholder="Username"
                                />
                            </div>
                            <div className="space-y-1">
                                <label className="text-xs font-medium uppercase text-muted-foreground">
                                    First name
                                </label>
                                <Input
                                    value={form.firstName}
                                    onChange={handleChange("firstName")}
                                    placeholder="First name"
                                />
                            </div>
                            <div className="space-y-1">
                                <label className="text-xs font-medium uppercase text-muted-foreground">
                                    Last name
                                </label>
                                <Input
                                    value={form.lastName}
                                    onChange={handleChange("lastName")}
                                    placeholder="Last name"
                                />
                            </div>
                            <div className="space-y-1">
                                <label className="text-xs font-medium uppercase text-muted-foreground">
                                    Phone number
                                </label>
                                <Input
                                    value={form.phoneNumber}
                                    onChange={handleChange("phoneNumber")}
                                    placeholder="+(123) 456-7890"
                                />
                            </div>
                        </div>

                        <div className="space-y-1">
                            <label className="text-xs font-medium uppercase text-muted-foreground">
                                About me
                            </label>
                            <textarea
                                className="w-full min-h-[120px] rounded-md border border-input bg-background px-3 py-2 text-sm"
                                value={form.aboutMe}
                                onChange={handleChange("aboutMe")}
                                placeholder="Write something..."
                            />
                        </div>

                        <div className="grid gap-4 md:grid-cols-2">
                            <div className="space-y-1">
                                <label className="text-xs font-medium uppercase text-muted-foreground">
                                    Quote
                                </label>
                                <Input
                                    value={form.quote}
                                    onChange={handleChange("quote")}
                                    placeholder="Your quote"
                                />
                            </div>

                            <div className="space-y-1">
                                <label className="text-xs font-medium uppercase text-muted-foreground">
                                    Gender
                                </label>
                                <select
                                    className="w-full rounded-md border border-input bg-background px-3 py-2 text-sm"
                                    value={form.gender}
                                    onChange={(e) =>
                                        setForm((prev) => ({ ...prev, gender: e.target.value }))
                                    }
                                >
                                    <option value="non">Non</option>
                                    <option value="male">Man</option>
                                    <option value="female">Woman</option>
                                    <option value="other">Other</option>
                                </select>
                            </div>

                            <div className="space-y-1">
                                <label className="text-xs font-medium uppercase text-muted-foreground">
                                    Date of birth
                                </label>
                                <Input
                                    type="date"
                                    value={form.dateOfBirth}
                                    onChange={handleChange("dateOfBirth")}
                                />
                            </div>

                            <div className="space-y-1">
                                <label className="text-xs font-medium uppercase text-muted-foreground">
                                    Country
                                </label>
                                <Input
                                    value={form.country}
                                    onChange={handleChange("country")}
                                    placeholder="Country"
                                />
                            </div>
                        </div>
                    </section>

                    {/* Avatar */}
                    <section className="space-y-3">
                        <h2 className="text-sm font-semibold tracking-wide uppercase text-muted-foreground">
                            Avatar
                        </h2>
                        <div className="space-y-1">
                            <label className="text-xs font-medium uppercase text-muted-foreground">
                                Upload avatar
                            </label>
                            <Input
                                type="file"
                                accept="image/*"
                                onChange={(e) => {
                                    const file = e.target.files?.[0] ?? null;
                                    setAvatarFile(file);
                                }}
                            />
                            <p className="text-xs text-muted-foreground">
                                File will be uploaded to media service via <code>/api/gw/media</code>, and
                                media id saved to <code>avatarMediaId</code>.
                            </p>
                        </div>
                    </section>

                    {/* Social */}
                    <section className="space-y-4">
                        <h2 className="text-sm font-semibold tracking-wide uppercase text-muted-foreground">
                            Social
                        </h2>
                        <div className="grid gap-4 md:grid-cols-2">
                            <div className="space-y-1">
                                <label className="text-xs font-medium uppercase text-muted-foreground">
                                    Facebook URL
                                </label>
                                <Input
                                    value={form.facebook}
                                    onChange={handleChange("facebook")}
                                    placeholder="https://facebook.com/..."
                                />
                            </div>
                            <div className="space-y-1">
                                <label className="text-xs font-medium uppercase text-muted-foreground">
                                    Twitter URL
                                </label>
                                <Input
                                    value={form.twitter}
                                    onChange={handleChange("twitter")}
                                    placeholder="https://twitter.com/..."
                                />
                            </div>
                            <div className="space-y-1">
                                <label className="text-xs font-medium uppercase text-muted-foreground">
                                    Instagram URL
                                </label>
                                <Input
                                    value={form.instagram}
                                    onChange={handleChange("instagram")}
                                    placeholder="https://instagram.com/..."
                                />
                            </div>
                            <div className="space-y-1">
                                <label className="text-xs font-medium uppercase text-muted-foreground">
                                    GitHub URL
                                </label>
                                <Input
                                    value={form.gitHub}
                                    onChange={handleChange("gitHub")}
                                    placeholder="https://github.com/..."
                                />
                            </div>
                        </div>
                    </section>

                    {/* Privacy */}
                    <section className="space-y-3">
                        <h2 className="text-sm font-semibold tracking-wide uppercase text-muted-foreground">
                            Privacy
                        </h2>
                        <div className="space-y-2 text-sm">
                            <label className="flex items-center gap-2">
                                <input
                                    type="checkbox"
                                    checked={form.personalInfoFriendsOnly}
                                    onChange={handleCheckbox("personalInfoFriendsOnly")}
                                />
                                <span>Your personal information can only be seen by your friends</span>
                            </label>

                            <label className="flex items-center gap-2">
                                <input
                                    type="checkbox"
                                    checked={form.showOnlyBasicInfo}
                                    onChange={handleCheckbox("showOnlyBasicInfo")}
                                />
                                <span>Show everyone only basic information about you</span>
                            </label>

                            <label className="flex items-center gap-2">
                                <input
                                    type="checkbox"
                                    checked={form.hideBrowsingHistory}
                                    onChange={handleCheckbox("hideBrowsingHistory")}
                                />
                                <span>Hide your browsing history</span>
                            </label>

                            <label className="flex items-center gap-2">
                                <input
                                    type="checkbox"
                                    checked={form.hideGrades}
                                    onChange={handleCheckbox("hideGrades")}
                                />
                                <span>Hide my grades</span>
                            </label>
                        </div>
                    </section>

                    <div className="flex justify-end gap-3 pt-4 border-t border-border/60">
                        <Button
                            type="button"
                            variant="outline"
                            onClick={() => router.push("/profile")}
                        >
                            Cancel
                        </Button>
                        <Button type="submit" disabled={saving}>
                            {saving ? "Saving…" : "Save"}
                        </Button>
                    </div>
                </form>
            </CardContent>
        </Card>
    );
}
