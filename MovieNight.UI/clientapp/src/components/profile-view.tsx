"use client";

import Link from "next/link";
import { useMemo } from "react";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Mail, Calendar, Phone, MapPin, Quote, User as UserIcon } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import { useBookmarks } from "@/lib/bookmarks-context";
import { useCurrentProfile } from "@/lib/use-current-profile";

export function ProfileView() {
    const { user, isLoading: authLoading } = useAuth();
    const { bookmarks } = useBookmarks();
    const { profile, loading } = useCurrentProfile();

    const isLoading = authLoading || loading;

    if (isLoading) {
        return (
            <Card className="bg-card/50 backdrop-blur">
                <CardContent className="p-8 text-center text-muted-foreground">
                    Loading profile…
                </CardContent>
            </Card>
        );
    }

    if (!user || !profile) {
        return (
            <Card className="bg-card/50 backdrop-blur">
                <CardContent className="p-12 text-center">
                    <p className="text-muted-foreground mb-4">
                        Please log in to view your profile.
                    </p>
                    <Link href="/">
                        <Button variant="outline">Go to home</Button>
                    </Link>
                </CardContent>
            </Card>
        );
    }

    const displayName =
        profile.userName || profile.displayName || user.name || profile.email || user.email;

    const fullName = useMemo(() => {
        const parts = [profile.firstName, profile.lastName].filter(Boolean);
        return parts.length ? parts.join(" ") : null;
    }, [profile.firstName, profile.lastName]);

    const memberSince = useMemo(() => {
        if (!profile.createdAt) return "—";
        const d = new Date(profile.createdAt);
        return d.toLocaleString(undefined, { month: "long", year: "numeric" });
    }, [profile.createdAt]);

    const dobFormatted = useMemo(() => {
        if (!profile.dateOfBirth) return null;
        const d = new Date(profile.dateOfBirth);
        if (Number.isNaN(d.getTime())) return null;
        return d.toLocaleDateString(undefined, {
            day: "2-digit",
            month: "long",
            year: "numeric",
        });
    }, [profile.dateOfBirth]);

    const avatarUrl =
        profile.avatarMediaId && profile.avatarMediaId.length > 0
            ? `/api/gw/media/${profile.avatarMediaId}` // предполагаемый урл из media-сервиса
            : profile.avatarUrl ?? null;

    return (
        <div className="max-w-6xl mx-auto space-y-8">
            <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
                <h1 className="text-4xl md:text-5xl font-serif font-bold">Profile</h1>
                <div className="flex gap-3">
                    <Link href="/profile/edit">
                        <Button variant="outline">
                            <UserIcon className="h-4 w-4 mr-2" />
                            Edit profile
                        </Button>
                    </Link>
                </div>
            </div>

            <div className="grid gap-6 md:grid-cols-[minmax(0,280px)_minmax(0,1fr)]">
                {/* Левая колонка: аватар, био, контакты, соцсети */}
                <div className="space-y-6">
                    <Card className="bg-card/60 backdrop-blur border-border">
                        <CardHeader className="flex flex-col items-center text-center gap-4">
                            <Avatar className="h-28 w-28">
                                {avatarUrl && <AvatarImage src={avatarUrl} alt={displayName ?? ""} />}
                                <AvatarFallback className="bg-primary text-primary-foreground text-3xl">
                                    {displayName?.charAt(0)?.toUpperCase()}
                                </AvatarFallback>
                            </Avatar>

                            <div>
                                <div className="text-sm text-muted-foreground mb-1">
                                    @{profile.userName || user.name || profile.email || user.email}
                                </div>
                                <h2 className="text-2xl font-serif font-bold">{displayName}</h2>

                                {profile.quote && (
                                    <p className="mt-3 text-sm text-muted-foreground flex items-center justify-center gap-2">
                                        <Quote className="h-4 w-4" />
                                        <span>{profile.quote}</span>
                                    </p>
                                )}
                            </div>
                        </CardHeader>
                        <CardContent className="space-y-4">
                            {profile.aboutMe && (
                                <div>
                                    <div className="text-xs uppercase tracking-wide text-muted-foreground mb-1">
                                        About me
                                    </div>
                                    <p className="text-sm leading-relaxed break-words whitespace-pre-wrap">
                                        {profile.aboutMe}
                                    </p>
                                </div>
                            )}

                            <div className="space-y-2 text-sm">
                                {fullName && (
                                    <div className="flex items-center gap-2">
                                        <UserIcon className="h-4 w-4 text-muted-foreground" />
                                        <span className="font-medium">{fullName}</span>
                                    </div>
                                )}

                                {profile.email && (
                                    <div className="flex items-center gap-2">
                                        <Mail className="h-4 w-4 text-muted-foreground" />
                                        <span>{profile.email}</span>
                                    </div>
                                )}

                                {profile.phoneNumber && (
                                    <div className="flex items-center gap-2">
                                        <Phone className="h-4 w-4 text-muted-foreground" />
                                        <span>{profile.phoneNumber}</span>
                                    </div>
                                )}

                                {profile.gender && (
                                    <div className="flex items-center gap-2">
                                        <UserIcon className="h-4 w-4 text-muted-foreground" />
                                        <span>{profile.gender}</span>
                                    </div>
                                )}

                                {dobFormatted && (
                                    <div className="flex items-center gap-2">
                                        <Calendar className="h-4 w-4 text-muted-foreground" />
                                        <span>{dobFormatted}</span>
                                    </div>
                                )}

                                {profile.country && (
                                    <div className="flex items-center gap-2">
                                        <MapPin className="h-4 w-4 text-muted-foreground" />
                                        <span>{profile.country}</span>
                                    </div>
                                )}
                            </div>

                            {/* Соцсети */}
                            {(profile.facebook ||
                                profile.instagram ||
                                profile.twitter ||
                                profile.gitHub) && (
                                <div className="space-y-2 pt-2 border-t border-border/60">
                                    <div className="text-xs uppercase tracking-wide text-muted-foreground">
                                        Social
                                    </div>
                                    <div className="flex flex-wrap gap-2">
                                        {profile.facebook && (
                                            <a
                                                href={profile.facebook}
                                                target="_blank"
                                                rel="noreferrer"
                                                className="text-xs px-2 py-1 rounded-full border border-border hover:bg-accent transition"
                                            >
                                                Facebook
                                            </a>
                                        )}
                                        {profile.instagram && (
                                            <a
                                                href={profile.instagram}
                                                target="_blank"
                                                rel="noreferrer"
                                                className="text-xs px-2 py-1 rounded-full border border-border hover:bg-accent transition"
                                            >
                                                Instagram
                                            </a>
                                        )}
                                        {profile.twitter && (
                                            <a
                                                href={profile.twitter}
                                                target="_blank"
                                                rel="noreferrer"
                                                className="text-xs px-2 py-1 rounded-full border border-border hover:bg-accent transition"
                                            >
                                                Twitter
                                            </a>
                                        )}
                                        {profile.gitHub && (
                                            <a
                                                href={profile.gitHub}
                                                target="_blank"
                                                rel="noreferrer"
                                                className="text-xs px-2 py-1 rounded-full border border-border hover:bg-accent transition"
                                            >
                                                GitHub
                                            </a>
                                        )}
                                    </div>
                                </div>
                            )}

                            {/* Настройки приватности (read-only отображение) */}
                            <div className="space-y-1 pt-2 border-t border-border/60 text-xs text-muted-foreground">
                                <div className="font-semibold text-[11px] uppercase tracking-wide">
                                    Privacy
                                </div>
                                <ul className="space-y-1">
                                    <li>
                                        Personal info visible only to friends:{" "}
                                        <span className="font-medium">
                      {profile.personalInfoFriendsOnly ? "Yes" : "No"}
                    </span>
                                    </li>
                                    <li>
                                        Show only basic info to others:{" "}
                                        <span className="font-medium">
                      {profile.showOnlyBasicInfo ? "Yes" : "No"}
                    </span>
                                    </li>
                                    <li>
                                        Hide browsing history:{" "}
                                        <span className="font-medium">
                      {profile.hideBrowsingHistory ? "Yes" : "No"}
                    </span>
                                    </li>
                                    <li>
                                        Hide grades:{" "}
                                        <span className="font-medium">
                      {profile.hideGrades ? "Yes" : "No"}
                    </span>
                                    </li>
                                </ul>
                            </div>
                        </CardContent>
                    </Card>
                </div>

                <div className="space-y-6">
                    <Card className="bg-card/60 backdrop-blur border-border">
                        <CardHeader>
                            <h3 className="text-lg font-semibold">Summary</h3>
                        </CardHeader>
                        <CardContent className="grid sm:grid-cols-3 gap-4">
                            <div className="p-3 rounded-lg bg-muted/40 border border-border">
                                <div className="text-xs uppercase text-muted-foreground mb-1">
                                    Member since
                                </div>
                                <div className="text-base font-medium">{memberSince}</div>
                            </div>

                            <div className="p-3 rounded-lg bg-muted/40 border border-border">
                                <div className="text-xs uppercase text-muted-foreground mb-1">
                                    Bookmarks
                                </div>
                                <div className="text-2xl font-bold">{bookmarks.length}</div>
                            </div>

                            <div className="p-3 rounded-lg bg-muted/40 border border-border">
                                <div className="text-xs uppercase text-muted-foreground mb-1">
                                    Roles
                                </div>
                                <div className="flex flex-wrap gap-1">
                                    {profile.roles?.length
                                        ? profile.roles.map((r) => (
                                            <Badge key={r} variant="secondary">
                                                {r}
                                            </Badge>
                                        ))
                                        : "-"}
                                </div>
                            </div>
                        </CardContent>
                    </Card>

                    <Card className="bg-card/60 backdrop-blur border-border">
                        <CardHeader>
                            <h3 className="text-lg font-semibold">Activity & stats</h3>
                        </CardHeader>
                        <CardContent className="text-sm text-muted-foreground space-y-2">
                            <p>
                                Here you can later plug in:
                            </p>
                            <ul className="list-disc list-inside space-y-1">
                                <li>Viewing history (timeline, last viewed)</li>
                                <li>“In the plans” / bookmarks list</li>
                                <li>Achievements with progress</li>
                                <li>Category stats (Anime/Films/Serials/Cartoons)</li>
                            </ul>
                            <p>
                                // Literal (word-for-word) translation:
                                // For this, a separate service (history/stats/achievements) will be useful, but
                                // the storage of the main profile has already been moved to Users and is available via
                                // /api/gw/users/me/profile.
                            </p>
                        </CardContent>
                    </Card>
                </div>
            </div>
        </div>
    );
}
