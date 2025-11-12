import { TopBar } from "@/components/top-bar"
import { MovieGrid } from "@/components/movie-grid"
import { mockNovelty } from "@/lib/mock-data"

export default function NoveltyPage() {
    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12">
                <MovieGrid title="New Releases" movies={mockNovelty} />
            </main>
        </div>
    )
}
