"use client"

import { useEffect, useState } from "react"
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"
import { Search, UserPlus } from "lucide-react"
import { useToast } from "@/hooks/use-toast"
import { useCurrentProfile } from "@/lib/use-current-profile"

interface AddFriendsDialogProps {
    open: boolean
    onOpenChange: (open: boolean) => void
    friends: { id: string }[]
    currentUserId?: string
}

type UserItem = {
    id: string
    name: string
    email: string
}

export function AddFriendsDialog({ open, onOpenChange, friends, currentUserId }: AddFriendsDialogProps) {
    const [searchQuery, setSearchQuery] = useState("")
    const [users, setUsers] = useState<UserItem[]>([])
    const [loading, setLoading] = useState(false)
    const [addingId, setAddingId] = useState<string | null>(null)
    const { toast } = useToast()
    const { profile } = useCurrentProfile()

    // грузим список пользователей при открытии диалога
    useEffect(() => {
        if (!open) return

        const loadUsers = async () => {
            try {
                setLoading(true)

                // предполагаю, что у тебя уже есть прокси /users на гейтвее
                const res = await fetch("/api/gw/users", {
                    credentials: "include",
                })

                if (!res.ok) {
                    throw new Error(`Failed to load users, status ${res.status}`)
                }

                const data = await res.json()

                const mapped: UserItem[] = data.map((u: any) => ({
                    id: u.id ?? u.userId,
                    name: u.userName ?? u.name ?? u.email,
                    email: u.email ?? "",
                }))

                setUsers(mapped)
            } catch (err) {
                console.error(err)
                toast({
                    title: "Error",
                    description: "Failed to load users list.",
                    variant: "destructive",
                })
            } finally {
                setLoading(false)
            }
        }

        loadUsers()
    }, [open, toast])
    
    const filteredUsers = users
        .filter(u => u.id !== currentUserId)                     // убрать текущего
        .filter(u => !friends.some(f => f.id === u.id))          // убрать друзей
        .filter(u =>
            u.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
            u.email.toLowerCase().includes(searchQuery.toLowerCase())
        )

    const handleAddFriend = async (user: UserItem) => {
        if (!profile?.id) {
            toast({
                title: "Not authorized",
                description: "Please log in first.",
                variant: "destructive",
            })
            return
        }

        try {
            setAddingId(user.id)

            const res = await fetch("/api/gw/friends", {
                method: "POST",
                credentials: "include",
                headers: {
                    "Content-Type": "application/json",
                },
                body: JSON.stringify({
                    // важно: имена полей под DTO на бэке (скорее всего PascalCase)
                    IdUser: profile.id,
                    IdFriend: user.id,
                    KindOfFriendship: 0,
                }),
            })

            if (!res.ok) {
                const text = await res.text()
                throw new Error(text || "Failed to add friend")
            }

            toast({
                title: "Friend added",
                description: `${user.name} has been added to your friends list.`,
            })
        } catch (err) {
            console.error(err)
            toast({
                title: "Error",
                description:
                    err instanceof Error ? err.message : "Failed to add friend.",
                variant: "destructive",
            })
        } finally {
            setAddingId(null)
        }
    }

    return (
        <Dialog open={open} onOpenChange={onOpenChange}>
            <DialogContent className="max-w-2xl max-h-[80vh] flex flex-col">
                <DialogHeader>
                    <DialogTitle className="text-2xl font-serif">Add Friends</DialogTitle>
                </DialogHeader>

                <div className="relative mb-4">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                    <Input
                        type="search"
                        placeholder="Search users..."
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                        className="pl-9"
                    />
                </div>

                {loading ? (
                    <p className="text-sm text-muted-foreground px-1">Loading users...</p>
                ) : (
                    <div className="flex-1 overflow-y-auto space-y-2 pr-2">
                        {filteredUsers.map((user) => (
                            <div
                                key={user.id}
                                className="flex items-center justify-between p-3 rounded-lg bg-muted/50 hover:bg-muted transition-colors"
                            >
                                <div className="flex items-center gap-3">
                                    <Avatar className="h-10 w-10">
                                        <AvatarFallback className="bg-primary text-primary-foreground">
                                            {user.name.charAt(0).toUpperCase()}
                                        </AvatarFallback>
                                    </Avatar>
                                    <div>
                                        <p className="font-medium">{user.name}</p>
                                        <p className="text-sm text-muted-foreground">{user.email}</p>
                                    </div>
                                </div>
                                <Button
                                    size="sm"
                                    variant="outline"
                                    className="gap-2 bg-transparent"
                                    onClick={() => handleAddFriend(user)}
                                    disabled={addingId === user.id}
                                >
                                    <UserPlus className="h-4 w-4" />
                                    {addingId === user.id ? "Adding..." : "Add"}
                                </Button>
                            </div>
                        ))}
                    </div>
                )}
            </DialogContent>
        </Dialog>
    )
}
