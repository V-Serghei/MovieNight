"use client"

import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"
import { Badge } from "@/components/ui/badge"
import { Mail, Calendar, Film } from "lucide-react"
import { useAuth } from "@/lib/auth-context"
import { useBookmarks } from "@/lib/bookmarks-context"

export function ProfileView() {
    const { user } = useAuth()
    const { bookmarks } = useBookmarks()

    if (!user) {
        return (
            <Card className="bg-card/50 backdrop-blur">
                <CardContent className="p-12 text-center">
                    <p className="text-muted-foreground">Please log in to view your profile</p>
                </CardContent>
            </Card>
        )
    }

    return (
        <div className="max-w-3xl mx-auto">
            <h1 className="text-4xl md:text-5xl font-serif font-bold mb-8">Profile</h1>

            <Card className="bg-card/50 backdrop-blur border-border">
                <CardHeader className="text-center pb-6">
                    <Avatar className="h-24 w-24 mx-auto mb-4">
                        <AvatarFallback className="bg-primary text-primary-foreground text-3xl">
                            {user.name.charAt(0)}
                        </AvatarFallback>
                    </Avatar>
                    <h2 className="text-2xl font-serif font-bold">{user.name}</h2>
                    <div className="flex items-center justify-center gap-2 text-muted-foreground mt-2">
                        <Mail className="h-4 w-4" />
                        {user.email}
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
                            <p className="text-lg font-medium">January 2025</p>
                        </div>
                    </div>

                    <div>
                        <h3 className="font-semibold mb-3">Favorite Genres</h3>
                        <div className="flex flex-wrap gap-2">
                            <Badge variant="secondary">Action</Badge>
                            <Badge variant="secondary">Sci-Fi</Badge>
                            <Badge variant="secondary">Drama</Badge>
                            <Badge variant="secondary">Thriller</Badge>
                        </div>
                    </div>
                </CardContent>
            </Card>
        </div>
    )
}
