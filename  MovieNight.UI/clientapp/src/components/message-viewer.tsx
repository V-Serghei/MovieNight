"use client"

import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Reply, Trash2, X } from "lucide-react"
import type { Message } from "@/lib/types"
import { useRouter } from "next/navigation"

interface MessageViewerProps {
    message: Message
    onDelete: () => void
    onClose: () => void
}

export function MessageViewer({ message, onDelete, onClose }: MessageViewerProps) {
    const router = useRouter()

    return (
        <Card className="bg-card/50 backdrop-blur border-border">
            <CardHeader className="border-b border-border">
                <div className="flex items-start justify-between gap-4">
                    <div className="flex-1 min-w-0">
                        <h2 className="text-xl font-serif font-bold text-balance mb-2">{message.subject}</h2>
                        <div className="flex items-center gap-2 text-sm text-muted-foreground">
                            <span className="font-medium text-foreground">{message.sender}</span>
                            <span>•</span>
                            <span>{message.date}</span>
                        </div>
                    </div>
                    <Button variant="ghost" size="icon" onClick={onClose} className="shrink-0">
                        <X className="h-4 w-4" />
                    </Button>
                </div>
            </CardHeader>
            <CardContent className="p-6">
                <p className="text-foreground leading-relaxed whitespace-pre-wrap">{message.body}</p>
            </CardContent>
            <CardFooter className="border-t border-border gap-2">
                <Button variant="default" className="gap-2" onClick={() => router.push("/messages/compose")}>
                    <Reply className="h-4 w-4" />
                    Reply
                </Button>
                <Button variant="outline" className="gap-2 bg-transparent" onClick={onDelete}>
                    <Trash2 className="h-4 w-4" />
                    Delete
                </Button>
            </CardFooter>
        </Card>
    )
}
