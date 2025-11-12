"use client"

import { useState } from "react"
import { Card, CardContent, CardHeader } from "@/components/ui/card"
import { Input } from "@/components/ui/input"
import { Badge } from "@/components/ui/badge"
import { Mail, Search } from "lucide-react"
import { mockMessages } from "@/lib/mock-data"
import type { Message } from "@/lib/types"
import { MessageViewer } from "@/components/message-viewer"

export function MessageList() {
    const [messages, setMessages] = useState(mockMessages)
    const [selectedMessage, setSelectedMessage] = useState<Message | null>(null)
    const [searchQuery, setSearchQuery] = useState("")

    const filteredMessages = messages.filter(
        (msg) =>
            msg.subject.toLowerCase().includes(searchQuery.toLowerCase()) ||
            msg.sender.toLowerCase().includes(searchQuery.toLowerCase()),
    )

    const handleDelete = (id: string) => {
        setMessages((prev) => prev.filter((msg) => msg.id !== id))
        if (selectedMessage?.id === id) {
            setSelectedMessage(null)
        }
    }

    return (
        <div>
            <div className="mb-8">
                <h1 className="text-4xl md:text-5xl font-serif font-bold mb-4">Inbox</h1>
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
                    {filteredMessages.length === 0 ? (
                        <Card className="bg-card/50 backdrop-blur">
                            <CardContent className="p-12 text-center">
                                <Mail className="h-12 w-12 mx-auto mb-4 text-muted-foreground" />
                                <p className="text-muted-foreground">No messages found</p>
                            </CardContent>
                        </Card>
                    ) : (
                        filteredMessages.map((message) => (
                            <Card
                                key={message.id}
                                className={`bg-card/50 backdrop-blur border-border cursor-pointer hover:border-primary/50 transition-colors ${
                                    selectedMessage?.id === message.id ? "border-primary" : ""
                                }`}
                                onClick={() => setSelectedMessage(message)}
                            >
                                <CardHeader className="pb-3">
                                    <div className="flex items-start justify-between gap-2">
                                        <div className="flex-1 min-w-0">
                                            <div className="flex items-center gap-2 mb-1">
                                                <p className="font-semibold truncate">{message.sender}</p>
                                                {!message.read && (
                                                    <Badge variant="default" className="shrink-0">
                                                        New
                                                    </Badge>
                                                )}
                                            </div>
                                            <h3 className="font-medium text-sm truncate">{message.subject}</h3>
                                        </div>
                                        <span className="text-xs text-muted-foreground shrink-0">{message.date}</span>
                                    </div>
                                </CardHeader>
                                <CardContent className="pt-0">
                                    <p className="text-sm text-muted-foreground line-clamp-2">{message.preview}</p>
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
                                <p className="text-muted-foreground">Select a message to read</p>
                            </CardContent>
                        </Card>
                    )}
                </div>
            </div>
        </div>
    )
}
