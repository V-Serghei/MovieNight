"use client"

import type React from "react"

import { useState } from "react"
import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Input } from "@/components/ui/input"
import { Label } from "@/components/ui/label"
import { Textarea } from "@/components/ui/textarea"
import { Send } from "lucide-react"
import { useToast } from "@/hooks/use-toast"
import { useRouter } from "next/navigation"

export function ComposeMessage() {
    const [recipient, setRecipient] = useState("")
    const [subject, setSubject] = useState("")
    const [body, setBody] = useState("")
    const { toast } = useToast()
    const router = useRouter()

    const handleSubmit = (e: React.FormEvent) => {
        e.preventDefault()

        if (!recipient || !subject || !body) {
            toast({
                title: "Missing fields",
                description: "Please fill in all required fields.",
                variant: "destructive",
            })
            return
        }

        console.log("[v0] Sending message:", { recipient, subject, body })
        toast({
            title: "Message sent",
            description: `Your message to ${recipient} has been sent successfully.`,
        })

        // Reset form
        setRecipient("")
        setSubject("")
        setBody("")

        // Navigate to inbox
        setTimeout(() => router.push("/messages/inbox"), 1000)
    }

    return (
        <div className="max-w-3xl mx-auto">
            <h1 className="text-4xl md:text-5xl font-serif font-bold mb-8">Compose Message</h1>

            <Card className="bg-card/50 backdrop-blur border-border">
                <form onSubmit={handleSubmit}>
                    <CardHeader>
                        <p className="text-muted-foreground">Send a message to another user</p>
                    </CardHeader>
                    <CardContent className="space-y-4">
                        <div className="space-y-2">
                            <Label htmlFor="recipient">Recipient *</Label>
                            <Input
                                id="recipient"
                                placeholder="Enter username..."
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
                        <Button type="submit" className="gap-2">
                            <Send className="h-4 w-4" />
                            Send Message
                        </Button>
                    </CardFooter>
                </form>
            </Card>
        </div>
    )
}
