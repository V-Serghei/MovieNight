"use client"

import { useState } from "react"
import { Dialog, DialogContent, DialogHeader, DialogTitle } from "@/components/ui/dialog"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"
import { Search, UserPlus } from "lucide-react"
import { mockAllUsers } from "@/lib/mock-data"
import { useToast } from "@/hooks/use-toast"

interface AddFriendsDialogProps {
    open: boolean
    onOpenChange: (open: boolean) => void
}

export function AddFriendsDialog({ open, onOpenChange }: AddFriendsDialogProps) {
    const [searchQuery, setSearchQuery] = useState("")
    const { toast } = useToast()

    const filteredUsers = mockAllUsers.filter((user) => user.name.toLowerCase().includes(searchQuery.toLowerCase()))

    const handleAddFriend = (name: string) => {
        console.log("[v0] Adding friend:", name)
        toast({
            title: "Friend added",
            description: `${name} has been added to your friends list.`,
        })
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

                <div className="flex-1 overflow-y-auto space-y-2 pr-2">
                    {filteredUsers.map((user) => (
                        <div
                            key={user.id}
                            className="flex items-center justify-between p-3 rounded-lg bg-muted/50 hover:bg-muted transition-colors"
                        >
                            <div className="flex items-center gap-3">
                                <Avatar className="h-10 w-10">
                                    <AvatarFallback className="bg-primary text-primary-foreground">{user.name.charAt(0)}</AvatarFallback>
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
                                onClick={() => handleAddFriend(user.name)}
                            >
                                <UserPlus className="h-4 w-4" />
                                Add
                            </Button>
                        </div>
                    ))}
                </div>
            </DialogContent>
        </Dialog>
    )
}
