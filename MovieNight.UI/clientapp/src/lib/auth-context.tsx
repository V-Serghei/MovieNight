"use client";

import React, {
    createContext,
    useContext,
    useEffect,
    useMemo,
    useState,
    useCallback,
} from "react";

export type AuthUser = {
    id: string;
    name: string;
    email: string;
    avatarUrl?: string | null;
    roles?: string[];
};

type AuthContextType = {
    user: AuthUser | null;
    isLoading: boolean;
    setUserFromMe: () => Promise<void>;
    loginLocal: (user: AuthUser | null) => void;
    logout: () => Promise<void>;
};

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: React.ReactNode }) {
    const [user, setUser] = useState<AuthUser | null>(null);
    const [isLoading, setIsLoading] = useState<boolean>(true);

    const setUserFromMe = useCallback(async () => {
        try {
            const res = await fetch("/api/gw/auth/me", {
                credentials: "include",
            });
            if (!res.ok) {
                setUser(null);
                return;
            }
            const data = await res.json(); // { user:{id,email,displayName,avatarUrl?}, exp? }
            const u = data?.user;
            if (u?.id) {
                setUser({
                    id: u.id,
                    email: u.email,
                    name: u.displayName ?? u.email,
                    avatarUrl: u.avatarUrl ?? null,
                });
            } else {
                setUser(null);
            }
        } catch {
            setUser(null);
        }
    }, []);

    useEffect(() => {
        (async () => {
            await setUserFromMe();
            setIsLoading(false);
        })();
    }, [setUserFromMe]);

    const loginLocal = useCallback((u: AuthUser | null) => {
        setUser(u);
    }, []);

    const logout = useCallback(async () => {
        try {
            await fetch("/api/gw/auth/logout", {
                method: "POST",
                credentials: "include",
            });
        } catch {
            // ignore
        } finally {
            setUser(null);
        }
    }, []);

    const value = useMemo(
        () => ({ user, isLoading, setUserFromMe, loginLocal, logout }),
        [user, isLoading, setUserFromMe, loginLocal, logout]
    );

    return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
    const ctx = useContext(AuthContext);
    if (!ctx) throw new Error("useAuth must be used within an AuthProvider");
    return ctx;
}
