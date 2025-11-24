"use client"

import { useEffect, useState } from "react"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"
import { Search, UserPlus, Mail, User, UserMinus, Users } from "lucide-react"
import { AddFriendsDialog } from "@/components/add-friends-dialog"
import { useRouter } from "next/navigation"
import { useToast } from "@/hooks/use-toast"
import { useCurrentProfile } from "@/lib/use-current-profile"

type FriendItem = {
    id: string
    name: string
    email: string
}

export function FriendsList() {
    const [friends, setFriends] = useState<FriendItem[]>([])
    const [searchQuery, setSearchQuery] = useState("")
    const [addDialogOpen, setAddDialogOpen] = useState(false)
    const [loading, setLoading] = useState(false)

    const router = useRouter()
    const { toast } = useToast()
    const { profile, loading: profileLoading } = useCurrentProfile()

    // грузим друзей текущего пользователя
    const loadFriends = async (userId: string) => {
        try {
            setLoading(true)

            // Gateway → FriendsProxyEndpoints → Friends API: GET /friends/{userId}
            const res = await fetch(`/api/gw/friends/${userId}`, {
                credentials: "include",
            })

            if (res.status === 404) {
                setFriends([])
                return
            }

            if (!res.ok) {
                throw new Error(`Failed to load friends. Status ${res.status}`)
            }

            const data = await res.json()

            // маппим ответ API под UI
            const mapped: FriendItem[] = data.map((f: any) => ({
                id: f.idFriend ?? f.id ?? "",
                name: f.friendName ?? f.name ?? f.idFriend ?? "Unknown user",
                email: f.friendEmail ?? f.email ?? "",
            }))

            setFriends(mapped)
        } catch (err) {
            console.error(err)
            toast({
                title: "Error",
                description: "Failed to load your friends list.",
                variant: "destructive",
            })
        } finally {
            setLoading(false)
        }
    }

    // первый загруз друзей, когда известен текущий пользователь
    useEffect(() => {
        if (profile?.id) {
            loadFriends(profile.id)
        }
    }, [profile?.id])

    // после закрытия AddFriendsDialog — перезагрузить друзей
    useEffect(() => {
        if (!addDialogOpen && profile?.id) {
            loadFriends(profile.id)
        }
    }, [addDialogOpen, profile?.id])

    const filteredFriends = friends.filter((friend) =>
        friend.name.toLowerCase().includes(searchQuery.toLowerCase())
    )

    const handleRemoveFriend = (id: string, name: string) => {
        // Сейчас только локально. Когда сделаешь DELETE /friends,
        // сюда можно будет добавить вызов:
        // await fetch(`/friends/${id}`, { method: "DELETE", ... })
        setFriends((prev) => prev.filter((f) => f.id !== id))
        toast({
            title: "Friend removed",
            description: `${name} has been removed from your friends list.`,
        })
    }

    if (profileLoading || loading) {
        return <p className="text-muted-foreground">Loading friends...</p>
    }

    return (
        <div>
            <div className="flex items-center justify-between mb-8">
                <h1 className="text-4xl md:text-5xl font-serif font-bold">My Friends</h1>
                <Button onClick={() => setAddDialogOpen(true)} className="gap-2">
                    <UserPlus className="h-4 w-4" />
                    Add Friends
                </Button>
            </div>

            <div className="mb-6">
                <div className="relative max-w-md">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                    <Input
                        type="search"
                        placeholder="Search friends..."
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                        className="pl-9"
                    />
                </div>
            </div>

            {filteredFriends.length === 0 ? (
                <Card className="bg-card/50 backdrop-blur">
                    <CardContent className="p-12 text-center">
                        <Users className="h-12 w-12 mx-auto mb-4 text-muted-foreground" />
                        <p className="text-muted-foreground mb-4">
                            {searchQuery ? "No friends found" : "You haven't added any friends yet"}
                        </p>
                        {!searchQuery && (
                            <Button onClick={() => setAddDialogOpen(true)} className="gap-2">
                                <UserPlus className="h-4 w-4" />
                                Add Friends
                            </Button>
                        )}
                    </CardContent>
                </Card>
            ) : (
                <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
                    {filteredFriends.map((friend) => (
                        <Card
                            key={friend.id}
                            className="bg-card/50 backdrop-blur border-border hover:border-primary/50 transition-colors"
                        >
                            <CardHeader className="text-center pb-3">
                                <Avatar className="h-20 w-20 mx-auto mb-3">
                                    <AvatarFallback className="bg-primary text-primary-foreground text-2xl">
                                        {friend.name.charAt(0)}
                                    </AvatarFallback>
                                </Avatar>
                                <h3 className="font-semibold text-lg">{friend.name}</h3>
                                <p className="text-sm text-muted-foreground">{friend.email}</p>
                            </CardHeader>
                            <CardContent className="space-y-2">
                                <Button
                                    variant="outline"
                                    className="w-full gap-2 bg-transparent"
                                    onClick={() => router.push("/messages/compose")}
                                >
                                    <Mail className="h-4 w-4" />
                                    Write message
                                </Button>
                                <Button
                                    variant="outline"
                                    className="w-full gap-2 bg-transparent"
                                    onClick={() => router.push("/profile")}
                                >
                                    <User className="h-4 w-4" />
                                    Go to profile
                                </Button>
                                <Button
                                    variant="outline"
                                    className="w-full gap-2 hover:bg-destructive hover:text-destructive-foreground bg-transparent"
                                    onClick={() => handleRemoveFriend(friend.id, friend.name)}
                                >
                                    <UserMinus className="h-4 w-4" />
                                    Remove friend
                                </Button>
                            </CardContent>
                        </Card>
                    ))}
                </div>
            )}

            {/* Диалог, в котором уже должны дергаться: 
                - GET /users  (список пользователей)
                - POST /friends (добавление в друзья через FriendsProxyEndpoints) */}
            <AddFriendsDialog
                open={addDialogOpen}
                onOpenChange={setAddDialogOpen}
                friends={friends}            
                currentUserId={profile?.id}  
            />

        </div>
    )
}
