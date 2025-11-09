"use client"

import type React from "react"

import { useState } from "react"
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useAuth } from "@/lib/auth-context"
import { useToast } from "@/hooks/use-toast"

interface RegisterDialogProps {
    open: boolean
    onOpenChange: (open: boolean) => void
    onOpenLogin?: () => void
}

export function RegisterDialog({ open, onOpenChange, onOpenLogin }: RegisterDialogProps) {
    const [email, setEmail] = useState("")
    const [displayName, setDisplayName] = useState("")
    const [password, setPassword] = useState("")
    const [confirm, setConfirm] = useState("")
    const { login } = useAuth()
    const { toast } = useToast()

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()

        if (!email || !password) {
            toast({ title: "Missing fields", description: "Email and password are required.", variant: "destructive" })
            return
        }
        if (password !== confirm) {
            toast({ title: "Passwords mismatch", description: "Please confirm your password.", variant: "destructive" })
            return
        }

        try {
            const res = await fetch(`/api/gw/auth/register`, {
                method: "POST",
                headers: { "content-type": "application/json" },
                body: JSON.stringify({ email, password, displayName: displayName || null }),
            })

            const text = await res.text()
            if (!res.ok) {
                toast({ title: "Registration failed", description: text || `HTTP ${res.status}`, variant: "destructive" })
                return
            }
            const data = JSON.parse(text) // { user:{id,email,displayName}, token?, exp? }

            login({ id: data.user.id, name: data.user.displayName ?? data.user.email, email: data.user.email })

            toast({ title: "Welcome!", description: "Your account has been created." })
            onOpenChange(false)
            setEmail("")
            setPassword("")
            setConfirm("")
            setDisplayName("")
        } catch (err: any) {
            toast({ title: "Network error", description: err?.message ?? String(err), variant: "destructive" })
        }
    }

    const openLogin = () => {
        if (onOpenLogin) {
            onOpenChange(false)
            onOpenLogin()
        }
    }

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-md">
                <DialogHeader>
                    <DialogTitle className="text-2xl font-serif">Create account</DialogTitle>
                </DialogHeader>

                <form onSubmit={handleSubmit} className="space-y-4">
                    <div className="space-y-2">
                        <Label htmlFor="reg-email">Email</Label>
                        <Input
                            id="reg-email"
                            type="email"
                            placeholder="your@email.com"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            required
                        />
                    </div>

                    <div className="space-y-2">
                        <Label htmlFor="reg-name">Display name (optional)</Label>
                        <Input
                            id="reg-name"
                            type="text"
                            placeholder="John"
                            value={displayName}
                            onChange={(e) => setDisplayName(e.target.value)}
                        />
                    </div>

                    <div className="space-y-2">
                        <Label htmlFor="reg-pass">Password</Label>
                        <Input
                            id="reg-pass"
                            type="password"
                            placeholder="••••••••"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            required
                        />
                    </div>

                    <div className="space-y-2">
                        <Label htmlFor="reg-confirm">Confirm password</Label>
                        <Input
                            id="reg-confirm"
                            type="password"
                            placeholder="••••••••"
                            value={confirm}
                            onChange={(e) => setConfirm(e.target.value)}
                            required
                        />
                    </div>

                    <Button type="submit" className="w-full">
                        Create account
                    </Button>

                    {onOpenLogin && (
                        <div className="text-sm text-center text-muted-foreground">
                            Already have an account?{" "}
                            <button
                                type="button"
                                onClick={openLogin}
                                className="text-primary hover:underline"
                            >
                                Log in
                            </button>
                        </div>
                    )}
                </form>
            </DialogContent>
        </Dialog>
    )
}
