import { TopBar } from "@/components/top-bar";
import { ProfileEditForm } from "@/components/profile-edit-form";

export default function ProfileEditPage() {
    return (
        <div className="min-h-screen">
            <TopBar />
            <main className="container mx-auto px-4 pt-24 pb-12">
                <ProfileEditForm />
            </main>
        </div>
    );
}
