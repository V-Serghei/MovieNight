import "./globals.css";
import { AuthProvider } from "@/lib/auth-context";
import { BookmarksProvider } from "@/lib/bookmarks-context";

export default function RootLayout({ children }: { children: React.ReactNode }) {
    return (
        <html lang="en">
        <body>
        <AuthProvider>
            <BookmarksProvider>
                {children}
            </BookmarksProvider>
        </AuthProvider>
        </body>
        </html>
    );
}
