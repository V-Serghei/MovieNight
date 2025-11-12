import { TopBar } from "@/components/top-bar"
import { ComposeMessage } from "@/components/compose-message"

export default function ComposePage() {
    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12">
                <ComposeMessage />
            </main>
        </div>
    )
}
