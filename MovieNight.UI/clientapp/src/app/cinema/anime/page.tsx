import { TopBar } from "@/components/top-bar"
import { MovieGrid } from "@/components/movie-grid"
import { mockAnime } from "@/lib/mock-data"

export default function AnimePage() {
    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12">
                <MovieGrid title="Anime" movies={mockAnime} />
            </main>
        </div>
    )
}
