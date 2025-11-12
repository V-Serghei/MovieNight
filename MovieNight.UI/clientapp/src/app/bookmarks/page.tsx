import { TopBar } from "@/components/top-bar"
import { BookmarksList } from "@/components/bookmarks-list"

export default function BookmarksPage() {
    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12">
                <BookmarksList />
            </main>
        </div>
    )
}
