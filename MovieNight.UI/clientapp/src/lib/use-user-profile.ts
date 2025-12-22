// clientapp/src/lib/use-user-profile.ts
"use client";

import { useEffect, useState } from "react";
import type { CurrentProfile } from "@/lib/use-current-profile";

export function useUserProfile(userId: string | null | undefined) {
    const [profile, setProfile] = useState<CurrentProfile | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!userId) {
            setProfile(null);
            setLoading(false);
            setError(null);
            return;
        }

        let cancelled = false;

        const run = async () => {
            setLoading(true);
            setError(null);

            try {
                const res = await fetch(`/api/gw/users/${userId}/profile`, {
                    credentials: "include",
                });

                if (!res.ok) {
                    if (!cancelled) {
                        setProfile(null);
                        setError(`HTTP ${res.status}`);
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
                    setError("network-error");
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
    }, [userId]);

    return { profile, loading, error };
}
