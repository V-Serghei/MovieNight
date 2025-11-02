import { TopBar } from "@/components/top-bar"
import { MovieGrid } from "@/components/movie-grid"
import { mockFilms } from "@/lib/mock-data"

export default function FilmsPage() {
    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12">
                <MovieGrid title="Films" movies={mockFilms} />
            </main>
        </div>
    )
}
