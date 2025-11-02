import { TopBar } from "@/components/top-bar"
import { MovieGrid } from "@/components/movie-grid"
import { mockSerials } from "@/lib/mock-data"

export default function SerialsPage() {
    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12">
                <MovieGrid title="Serials" movies={mockSerials} />
            </main>
        </div>
    )
}
