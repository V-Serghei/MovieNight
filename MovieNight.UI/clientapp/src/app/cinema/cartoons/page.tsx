import { TopBar } from "@/components/top-bar"
import { MovieGrid } from "@/components/movie-grid"
import { mockCartoons } from "@/lib/mock-data"

export default function CartoonsPage() {
    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12">
                <MovieGrid title="Cartoons" movies={mockCartoons} />
            </main>
        </div>
    )
}
