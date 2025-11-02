import { TopBar } from "@/components/top-bar"
import { FriendsList } from "@/components/friends-list"

export default function FriendsPage() {
    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12">
                <FriendsList />
            </main>
        </div>
    )
}
