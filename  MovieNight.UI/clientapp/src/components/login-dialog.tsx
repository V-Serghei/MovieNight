"use client"

import type React from "react"

import { useState } from "react"
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { useAuth } from "@/lib/auth-context"
import { useToast } from "@/hooks/use-toast"

interface LoginDialogProps {
    open: boolean
    onOpenChange: (open: boolean) => void
    onOpenRegister?: () => void
}

export function LoginDialog({ open, onOpenChange, onOpenRegister }: LoginDialogProps) {
    const [email, setEmail] = useState("")
    const [password, setPassword] = useState("")
    const { login } = useAuth()
    const { toast } = useToast()

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()

        if (!email || !password) {
            toast({ title: "Missing fields", description: "Please enter both email and password.", variant: "destructive" })
            return
        }

        try {
            const res = await fetch(`/api/gw/auth/login`, {
                method: "POST",
                headers: { "content-type": "application/json" },
                body: JSON.stringify({ email, password }),
            })

            const text = await res.text()
            if (!res.ok) {
                toast({ title: "Login failed", description: text || `HTTP ${res.status}`, variant: "destructive" })
                return
            }
            const data = JSON.parse(text) // { user:{id,email,displayName}, token?, exp? }

            login({ id: data.user.id, name: data.user.displayName ?? data.user.email, email: data.user.email })

            toast({ title: "Welcome back!", description: "You have successfully logged in." })
            onOpenChange(false)
            setEmail("")
            setPassword("")
        } catch (err: any) {
            toast({ title: "Network error", description: err?.message ?? String(err), variant: "destructive" })
        }
    }

    const openRegister = () => {
        if (onOpenRegister) {
            onOpenChange(false)
            onOpenRegister()
        }
    }

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="sm:max-w-md">
                <DialogHeader>
                    <DialogTitle className="text-2xl font-serif">Log in</DialogTitle>
                </DialogHeader>
                <form onSubmit={handleSubmit} className="space-y-4">
                    <div className="space-y-2">
                        <Label htmlFor="email">Email</Label>
                        <Input
                            id="email"
                            type="email"
                            placeholder="your@email.com"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            required
                        />
                    </div>
                    <div className="space-y-2">
                        <Label htmlFor="password">Password</Label>
                        <Input
                            id="password"
                            type="password"
                            placeholder="••••••••"
                            value={password}
                            onChange={(e) => setPassword(e.target.value)}
                            required
                        />
                    </div>

                    <Button type="submit" className="w-full">
                        Log in
                    </Button>

                    {onOpenRegister && (
                        <div className="text-sm text-center text-muted-foreground">
                            Don&apos;t have an account?{" "}
                            <button
                                type="button"
                                onClick={openRegister}
                                className="text-primary hover:underline"
                            >
                                Create account
                            </button>
                        </div>
                    )}
                </form>
            </DialogContent>
        </Dialog>
    )
}
