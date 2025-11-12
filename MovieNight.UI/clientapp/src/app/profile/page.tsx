import { TopBar } from "@/components/top-bar"
import { ProfileView } from "@/components/profile-view"

export default function ProfilePage() {
    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12">
                <ProfileView />
            </main>
        </div>
    )
}
