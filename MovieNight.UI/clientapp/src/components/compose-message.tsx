"use client"

import type React from "react"

import { useEffect, useState } from "react"
import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Send, Search } from "lucide-react"
import { useToast } from "@/hooks/use-toast"
import { useRouter } from "next/navigation"
import { useCurrentProfile } from "@/lib/use-current-profile"
import {
    Dialog,
    DialogContent,
    DialogHeader,
    DialogTitle,
} from "@/components/ui/dialog"
import { Avatar, AvatarFallback } from "@/components/ui/avatar"

type MessagesRequest = {
    isChecked: boolean
    senderName: string
    senderId: string
    recipientName: string
    recipientId: string
    theme: string
    message: string
    date: string
    isStarred: boolean
}

type UserItem = {
    id: string
    name: string
    email: string
}

export function ComposeMessage() {
    const [recipient, setRecipient] = useState("")
    const [subject, setSubject] = useState("")
    const [body, setBody] = useState("")
    const [isSending, setIsSending] = useState(false)

    const [users, setUsers] = useState<UserItem[]>([])
    const [usersLoading, setUsersLoading] = useState(false)

    const [pickerOpen, setPickerOpen] = useState(false)
    const [pickerSearch, setPickerSearch] = useState("")

    const { toast } = useToast()
    const router = useRouter()
    const { profile, loading } = useCurrentProfile()

    // грузим список всех пользователей (как в AddFriendsDialog)
    useEffect(() => {
        const loadUsers = async () => {
            try {
                setUsersLoading(true)

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
                setUsersLoading(false)
            }
        }

        loadUsers()
    }, [toast])

    const filteredUsers = users.filter(
        (u) =>
            u.name.toLowerCase().includes(pickerSearch.toLowerCase()) ||
            u.email.toLowerCase().includes(pickerSearch.toLowerCase()),
    )

    const handleSelectUser = (user: UserItem) => {
        // подставляем в поле получателя понятное значение
        setRecipient(user.name || user.email || "")
        setPickerOpen(false)
    }

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault()

        if (!recipient || !subject || !body) {
            toast({
                title: "Missing fields",
                description: "Please fill in all required fields.",
                variant: "destructive",
            })
            return
        }

        if (loading || usersLoading) {
            toast({
                title: "Please wait",
                description: "Loading your profile or users list...",
            })
            return
        }

        if (!profile) {
            toast({
                title: "Not authenticated",
                description: "You must be logged in to send messages.",
                variant: "destructive",
            })
            return
        }

        if (!users.length) {
            // пользователей ещё не загрузили — просто не шлём запрос
            setPickerOpen(true)
            return
        }

        try {
            setIsSending(true)

            // ищем пользователя по имени / e-mail
            const recipientInput = recipient.trim().toLowerCase()
            const matchedUser = users.find(
                (u) =>
                    u.name.toLowerCase() === recipientInput ||
                    u.email.toLowerCase() === recipientInput,
            )

            if (!matchedUser) {
                // НИЧЕГО НЕ ШЛЁМ, просто показываем окно выбора
                setPickerSearch(recipient) // можно подсветить ввод
                setPickerOpen(true)
                setIsSending(false)
                return
            }

            if (matchedUser.id === profile.id) {
                toast({
                    title: "Invalid recipient",
                    description: "You cannot send messages to yourself.",
                    variant: "destructive",
                })
                setIsSending(false)
                return
            }

            const senderId = profile.id
            const senderName =
                profile.displayName ??
                profile.email ??
                "Unknown"

            const payload: MessagesRequest = {
                isChecked: false,
                senderName,
                senderId,
                recipientName: matchedUser.name,
                recipientId: matchedUser.id,
                theme: subject,
                message: body,
                date: new Date().toISOString(),
                isStarred: false,
            }

            const resp = await fetch(`/api/gw/messages/compose`, {
                method: "POST",
                headers: {
                    "Content-Type": "application/json",
                },
                credentials: "include",
                body: JSON.stringify(payload),
            })

            if (!resp.ok) {
                const text = await resp.text()
                console.error("Failed to send message:", resp.status, text)
                toast({
                    title: "Error",
                    description: "Failed to send message. Try again later.",
                    variant: "destructive",
                })
                return
            }

            toast({
                title: "Message sent",
                description: `Your message to ${matchedUser.name} has been sent successfully.`,
            })

            setRecipient("")
            setSubject("")
            setBody("")

            const receiverId = profile.id
            // идём на UI, а не на /api/gw/...
            router.push("/messages/inbox")
        } catch (err) {
            console.error(err)
            toast({
                title: "Unexpected error",
                description: "Something went wrong while sending the message.",
                variant: "destructive",
            })
        } finally {
            setIsSending(false)
        }
    }

    if (loading || usersLoading) {
        return <div className="max-w-3xl mx-auto">Loading...</div>
    }

    return (
        <div className="max-w-3xl mx-auto">
            <h1 className="text-4xl md:text-5xl font-serif font-bold mb-8">
                Compose Message
            </h1>

            <Card className="bg-card/50 backdrop-blur border-border">
                <form onSubmit={handleSubmit}>
                    <CardHeader>
                        <p className="text-muted-foreground">
                            Send a message to another user
                        </p>
                        <p className="text-xs text-muted-foreground mt-1">
                            You can enter username or email. If we cannot match it, we&apos;ll show you the full users list to pick from.
                        </p>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <div className="space-y-2">
                            <Label htmlFor="recipient">Recipient *</Label>
                            <Input
                                id="recipient"
                                placeholder="Enter username or email..."
                                value={recipient}
                                onChange={(e) => setRecipient(e.target.value)}
                                required
                            />
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="subject">Subject *</Label>
                            <Input
                                id="subject"
                                placeholder="Enter subject..."
                                value={subject}
                                onChange={(e) => setSubject(e.target.value)}
                                required
                            />
                        </div>

                        <div className="space-y-2">
                            <Label htmlFor="body">Message *</Label>
                            <Textarea
                                id="body"
                                placeholder="Write your message..."
                                value={body}
                                onChange={(e) => setBody(e.target.value)}
                                rows={8}
                                required
                            />
                        </div>
                    </CardContent>
                    <CardFooter className="border-t border-border">
                        <Button type="submit" className="gap-2" disabled={isSending}>
                            <Send className="h-4 w-4" />
                            {isSending ? "Sending..." : "Send Message"}
                        </Button>
                    </CardFooter>
                </form>
            </Card>

            {/* Выдвигающееся окно с пользователями */}
            <Dialog open={pickerOpen} onOpenChange={setPickerOpen}>
                <DialogContent className="max-w-2xl max-h-[80vh] flex flex-col">
                    <DialogHeader>
                        <DialogTitle className="text-2xl font-serif">
                            Select recipient
                        </DialogTitle>
                    </DialogHeader>

                    <div className="relative mb-4">
                        <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                        <Input
                            type="search"
                            placeholder="Search users..."
                            value={pickerSearch}
                            onChange={(e) => setPickerSearch(e.target.value)}
                            className="pl-9"
                        />
                    </div>

                    <div className="flex-1 overflow-y-auto space-y-2 pr-2">
                        {filteredUsers.map((user) => (
                            <button
                                key={user.id}
                                type="button"
                                onClick={() => handleSelectUser(user)}
                                className="flex w-full items-center justify-between p-3 rounded-lg bg-muted/50 hover:bg-muted transition-colors text-left"
                            >
                                <div className="flex items-center gap-3">
                                    <Avatar className="h-10 w-10">
                                        <AvatarFallback className="bg-primary text-primary-foreground">
                                            {user.name.charAt(0).toUpperCase()}
                                        </AvatarFallback>
                                    </Avatar>
                                    <div>
                                        <p className="font-medium">{user.name}</p>
                                        <p className="text-sm text-muted-foreground">
                                            {user.email}
                                        </p>
                                    </div>
                                </div>
                            </button>
                        ))}
                        {!filteredUsers.length && (
                            <p className="text-sm text-muted-foreground px-1">
                                No users found.
                            </p>
                        )}
                    </div>
                </DialogContent>
            </Dialog>
        </div>
    )
}
