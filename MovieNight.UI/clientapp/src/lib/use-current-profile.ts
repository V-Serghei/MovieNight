"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/lib/auth-context";

export type CurrentProfile = {
    id: string;
    email?: string;
    displayName?: string;
    createdAt?: string;
    avatarUrl?: string | null;
    roles?: string[];
};

export function useCurrentProfile() {
    const { user } = useAuth();
    const [profile, setProfile] = useState<CurrentProfile | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        let cancelled = false;

        const run = async () => {
            if (!user) {
                if (!cancelled) {
                    setProfile(null);
                    setLoading(false);
                }
                return;
            }

            setLoading(true);

            try {
                const res = await fetch("/api/gw/users/me", {
                    credentials: "include",
                });

                if (!res.ok) {
                    if (!cancelled) {
                        setProfile(null);
                    }
                    return;
                }

                const data = (await res.json()) as CurrentProfile;
                if (!cancelled) {
                    setProfile(data);
                }
            } catch {
                if (!cancelled) {
                    setProfile(null);
                }
            } finally {
                if (!cancelled) {
                    setLoading(false);
                }
            }
        };

        run();
        return () => {
            cancelled = true;
        };
    }, [user]);

    const hasAdminAccess =
        !!profile?.roles?.some(
            (r) => r === "admin" || r === "moderator"
        );

    return { profile, loading, hasAdminAccess };
}
