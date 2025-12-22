import { TopBar } from "@/components/top-bar"
import { RandomMovie } from "@/components/random-movie"

export default function RandomPage() {
    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12">
                <RandomMovie />
            </main>
        </div>
    )
}
