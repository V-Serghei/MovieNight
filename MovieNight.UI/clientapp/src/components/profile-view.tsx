"use client";

import { useEffect, useState } from "react";
import { Card, CardContent, CardHeader } from "@/components/ui/card";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Mail, Calendar, Film } from "lucide-react";
import { useAuth } from "@/lib/auth-context";
import { useBookmarks } from "@/lib/bookmarks-context";

type PublicProfile = {
    id: string;
    email?: string;
    displayName?: string;
    createdAt?: string; // ISO
    avatarUrl?: string | null;
    roles?: string[];
};

export function ProfileView() {
    const { user } = useAuth();
    const { bookmarks } = useBookmarks();
    const [profile, setProfile] = useState<PublicProfile | null>(null);

    useEffect(() => {
        let cancelled = false;
        const run = async () => {
            if (!user) {
                setProfile(null);
                return;
            }
            try {
                const res = await fetch("/api/gw/users/me", { credentials: "include" });
                if (!res.ok) {
                    setProfile(null);
                    return;
                }
                const data = (await res.json()) as PublicProfile;
                if (!cancelled) setProfile(data);
            } catch {
                if (!cancelled) setProfile(null);
            }
        };
        run();
        return () => {
            cancelled = true;
        };
    }, [user]);

    if (!user) {
        return (
            <Card className="bg-card/50 backdrop-blur">
                <CardContent className="p-12 text-center">
                    <p className="text-muted-foreground">Please log in to view your profile</p>
                </CardContent>
            </Card>
        );
    }

    const displayName = profile?.displayName ?? user.name;
    const email = profile?.email ?? user.email;
    const createdAt = profile?.createdAt ? new Date(profile.createdAt) : null;
    const memberSince = createdAt
        ? createdAt.toLocaleString(undefined, { month: "long", year: "numeric" })
        : "—";

    return (
        <div className="max-w-3xl mx-auto">
            <h1 className="text-4xl md:text-5xl font-serif font-bold mb-8">Profile</h1>

            <Card className="bg-card/50 backdrop-blur border-border">
                <CardHeader className="text-center pb-6">
                    <Avatar className="h-24 w-24 mx-auto mb-4">
                        <AvatarFallback className="bg-primary text-primary-foreground text-3xl">
                            {displayName?.charAt(0)?.toUpperCase()}
                        </AvatarFallback>
                    </Avatar>
                    <h2 className="text-2xl font-serif font-bold">{displayName}</h2>
                    <div className="flex items-center justify-center gap-2 text-muted-foreground mt-2">
                        <Mail className="h-4 w-4" />
                        {email}
                    </div>
                </CardHeader>
                <CardContent className="space-y-6">
                    <div className="grid gap-4 sm:grid-cols-2">
                        <div className="p-4 rounded-lg bg-muted/50 border border-border">
                            <div className="flex items-center gap-3 mb-2">
                                <Film className="h-5 w-5 text-primary" />
                                <span className="font-semibold">Bookmarks</span>
                            </div>
                            <p className="text-2xl font-bold">{bookmarks.length}</p>
                        </div>

                        <div className="p-4 rounded-lg bg-muted/50 border border-border">
                            <div className="flex items-center gap-3 mb-2">
                                <Calendar className="h-5 w-5 text-primary" />
                                <span className="font-semibold">Member Since</span>
                            </div>
                            <p className="text-lg font-medium">{memberSince}</p>
                        </div>
                    </div>

                    {Array.isArray(profile?.roles) && profile!.roles!.length > 0 && (
                        <div>
                            <h3 className="font-semibold mb-3">Roles</h3>
                            <div className="flex flex-wrap gap-2">
                                {profile!.roles!.map((r) => (
                                    <Badge key={r} variant="secondary">
                                        {r}
                                    </Badge>
                                ))}
                            </div>
                        </div>
                    )}
                </CardContent>
            </Card>
        </div>
    );
}
