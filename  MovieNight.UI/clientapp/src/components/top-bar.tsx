"use client"

import type React from "react"

import { useState } from "react"
import Link from "next/link"
import { Menu, Search, Bookmark, LogIn, User, LogOut, Film } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Sheet, SheetContent, SheetTrigger } from "@/components/ui/sheet"
import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"
import { SideNav } from "@/components/side-nav"
import { useAuth } from "@/lib/auth-context"
import { LoginDialog } from "@/components/login-dialog"
import { useRouter } from "next/navigation"

export function TopBar() {
    const [searchQuery, setSearchQuery] = useState("")
    const [loginOpen, setLoginOpen] = useState(false)
    const { user, logout } = useAuth()
    const router = useRouter()

    const handleSearch = (e: React.FormEvent) => {
        e.preventDefault()
        if (searchQuery.trim()) {
            console.log("[v0] Searching for:", searchQuery)
            // In a real app, navigate to search results
        }
    }

    return (
        <>
            <header className="fixed top-0 left-0 right-0 z-50 border-b border-border bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/80">
                <div className="container mx-auto px-4 h-16 flex items-center justify-between gap-4">
                    {/* Left: Logo and Menu */}
                    <div className="flex items-center gap-3">
                        <Sheet>
                            <SheetTrigger asChild>
                                <Button variant="ghost" size="icon" className="shrink-0">
                                    <Menu className="h-5 w-5" />
                                    <span className="sr-only">Open menu</span>
                                </Button>
                            </SheetTrigger>
                            <SheetContent side="left" className="w-72 p-0">
                                <SideNav />
                            </SheetContent>
                        </Sheet>

                        <Link href="/" className="flex items-center gap-2 shrink-0">
                            <Film className="h-6 w-6 text-primary" />
                            <span className="font-serif font-bold text-xl hidden sm:inline">Movie Night</span>
                        </Link>
                    </div>

                    {/* Center: Search */}
                    <form onSubmit={handleSearch} className="flex-1 max-w-md">
                        <div className="relative">
                            <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                            <Input
                                type="search"
                                placeholder="Search movies..."
                                value={searchQuery}
                                onChange={(e) => setSearchQuery(e.target.value)}
                                className="pl-9 bg-muted/50"
                            />
                        </div>
                    </form>

                    {/* Right: Bookmarks and Auth */}
                    <div className="flex items-center gap-2 shrink-0">
                        <Button variant="ghost" size="icon" asChild className="hover:text-primary">
                            <Link href="/bookmarks">
                                <Bookmark className="h-5 w-5" />
                                <span className="sr-only">Bookmarks</span>
                            </Link>
                        </Button>

                        {user ? (
                            <DropdownMenu>
                                <DropdownMenuTrigger asChild>
                                    <Button variant="ghost" size="icon" className="rounded-full">
                                        <Avatar className="h-8 w-8">
                                            <AvatarFallback className="bg-primary text-primary-foreground">
                                                {user.name.charAt(0)}
                                            </AvatarFallback>
                                        </Avatar>
                                    </Button>
                                </DropdownMenuTrigger>
                                <DropdownMenuContent align="end" className="w-48">
                                    <DropdownMenuItem onClick={() => router.push("/profile")}>
                                        <User className="mr-2 h-4 w-4" />
                                        Profile
                                    </DropdownMenuItem>
                                    <DropdownMenuItem onClick={logout}>
                                        <LogOut className="mr-2 h-4 w-4" />
                                        Logout
                                    </DropdownMenuItem>
                                </DropdownMenuContent>
                            </DropdownMenu>
                        ) : (
                            <Button variant="default" size="sm" onClick={() => setLoginOpen(true)} className="gap-2">
                                <LogIn className="h-4 w-4" />
                                <span className="hidden sm:inline">Log in</span>
                            </Button>
                        )}
                    </div>
                </div>
            </header>

            <LoginDialog open={loginOpen} onOpenChange={setLoginOpen} />
        </>
    )
}
