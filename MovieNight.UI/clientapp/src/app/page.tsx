import { TopBar } from "@/components/top-bar"
import { NewsCard } from "@/components/news-card"
import { mockNews } from "@/lib/mock-data"

export default function HomePage() {
    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12">
                <div className="mb-8">
                    <h1 className="text-4xl md:text-5xl font-serif font-bold text-balance mb-3">Welcome to Movie Night</h1>
                    <p className="text-muted-foreground text-lg">Your ultimate destination for cinema entertainment</p>
                </div>

                <div className="grid gap-6 md:grid-cols-2 lg:grid-cols-3">
                    {mockNews.map((news) => (
                        <NewsCard key={news.id} news={news} />
                    ))}
                </div>
            </main>
        </div>
    )
}
