import { TopBar } from "@/components/top-bar"
import { MessageList } from "@/components/message-list"

export default function InboxPage() {
    return (
        <div className="min-h-screen">
        <TopBar />
        <main className="container mx-auto px-4 pt-24 pb-12">
            <MessageList />
            </main>
            </div>
    )
}
