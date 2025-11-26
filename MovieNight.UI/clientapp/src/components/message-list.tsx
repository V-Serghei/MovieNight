"use client"

import { useEffect, useState } from "react"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Badge } from "@/components/ui/badge"
import { Mail, Search } from "lucide-react"
import type { Message } from "@/lib/types"
import { MessageViewer } from "@/components/message-viewer"
import { useCurrentProfile } from "@/lib/use-current-profile"
import { useToast } from "@/hooks/use-toast"

type ApiMessage = {
    id: string
    senderName: string
    senderId: string
    recipientName: string
    recipientId: string
    theme: string
    message: string
    date: string
    isChecked: boolean
    isStarred: boolean
}

export function MessageList() {
    const [messages, setMessages] = useState<Message[]>([])
    const [selectedMessage, setSelectedMessage] = useState<Message | null>(null)
    const [searchQuery, setSearchQuery] = useState("")
    const [isLoading, setIsLoading] = useState(true)

    const { profile, loading: profileLoading } = useCurrentProfile()
    const { toast } = useToast()

    useEffect(() => {
        const load = async () => {
            if (profileLoading) return
            if (!profile) {
                setIsLoading(false)
                return
            }

            try {
                setIsLoading(true)
                
                const resp = await fetch(
                    `/api/gw/messages/by-receiver/${profile.id}`,
                    {
                        method: "GET",
                        credentials: "include",
                    },
                )

                if (!resp.ok) {
                    const text = await resp.text()
                    console.error("Failed to load messages:", resp.status, text)
                    toast({
                        title: "Error",
                        description: "Failed to load messages from server.",
                        variant: "destructive",
                    })
                    setIsLoading(false)
                    return
                }

                const apiData: ApiMessage[] = await resp.json()

                const mapped: Message[] = apiData.map((m) => ({
                    id: m.id,
                    subject: m.theme,
                    sender: m.senderName,
                    date: new Date(m.date).toLocaleString(),
                    preview:
                        m.message.length > 140
                            ? m.message.slice(0, 140) + "..."
                            : m.message,
                    body: m.message,
                    read: m.isChecked,
                }))

                setMessages(mapped)
            } catch (err) {
                console.error(err)
                toast({
                    title: "Unexpected error",
                    description: "Could not load messages.",
                    variant: "destructive",
                })
            } finally {
                setIsLoading(false)
            }
        }

        load()
    }, [profileLoading, profile, toast])

    const filteredMessages = messages.filter(
        (msg) =>
            msg.subject.toLowerCase().includes(searchQuery.toLowerCase()) ||
            msg.sender.toLowerCase().includes(searchQuery.toLowerCase()),
    )

    const handleDelete = (id: string) => {
        // пока удаляем только на клиенте, на бэке delete нет
        setMessages((prev) => prev.filter((msg) => msg.id !== id))
        if (selectedMessage?.id === id) {
            setSelectedMessage(null)
        }
    }

    return (
        <div>
            <div className="mb-8">
                <h1 className="text-4xl md:text-5xl font-serif font-bold mb-4">
                    Inbox
                </h1>
                <div className="relative max-w-md">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-muted-foreground" />
                    <Input
                        type="search"
                        placeholder="Search messages..."
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                        className="pl-9"
                    />
                </div>
            </div>

            <div className="grid lg:grid-cols-2 gap-6">
                <div className="space-y-3">
                    {isLoading ? (
                        <Card className="bg-card/50 backdrop-blur">
                            <CardContent className="p-12 text-center">
                                <p className="text-muted-foreground">
                                    Loading messages...
                                </p>
                            </CardContent>
                        </Card>
                    ) : filteredMessages.length === 0 ? (
                        <Card className="bg-card/50 backdrop-blur">
                            <CardContent className="p-12 text-center">
                                <Mail className="h-12 w-12 mx-auto mb-4 text-muted-foreground" />
                                <p className="text-muted-foreground">
                                    No messages found
                                </p>
                            </CardContent>
                        </Card>
                    ) : (
                        filteredMessages.map((message) => (
                            <Card
                                key={message.id}
                                className={`bg-card/50 backdrop-blur border-border cursor-pointer hover:border-primary/50 transition-colors ${
                                    selectedMessage?.id === message.id
                                        ? "border-primary"
                                        : ""
                                }`}
                                onClick={() => setSelectedMessage(message)}
                            >
                                <CardHeader className="pb-3">
                                    <div className="flex items-start justify-between gap-2">
                                        <div className="flex-1 min-w-0">
                                            <div className="flex items-center gap-2 mb-1">
                                                <p className="font-semibold truncate">
                                                    {message.sender}
                                                </p>
                                                {!message.read && (
                                                    <Badge
                                                        variant="default"
                                                        className="shrink-0"
                                                    >
                                                        New
                                                    </Badge>
                                                )}
                                            </div>
                                            <h3 className="font-medium text-sm truncate">
                                                {message.subject}
                                            </h3>
                                        </div>
                                        <span className="text-xs text-muted-foreground shrink-0">
                                            {message.date}
                                        </span>
                                    </div>
                                </CardHeader>
                                <CardContent className="pt-0">
                                    <p className="text-sm text-muted-foreground line-clamp-2">
                                        {message.preview}
                                    </p>
                                </CardContent>
                            </Card>
                        ))
                    )}
                </div>

                <div className="lg:sticky lg:top-24 lg:self-start">
                    {selectedMessage ? (
                        <MessageViewer
                            message={selectedMessage}
                            onDelete={() => handleDelete(selectedMessage.id)}
                            onClose={() => setSelectedMessage(null)}
                        />
                    ) : (
                        <Card className="bg-card/50 backdrop-blur">
                            <CardContent className="p-12 text-center">
                                <Mail className="h-12 w-12 mx-auto mb-4 text-muted-foreground" />
                                <p className="text-muted-foreground">
                                    Select a message to read
                                </p>
                            </CardContent>
                        </Card>
                    )}
                </div>
            </div>
        </div>
    )
}
