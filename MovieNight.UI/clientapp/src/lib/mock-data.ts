import type { Movie, News, Message, User } from "@/lib/types"

export const mockFilms: Movie[] = [
    {
        id: "f1",
        title: "Inception",
        poster: "/inception-movie-poster.png",
        rating: 8.8,
        year: 2010,
        duration: "2h 28m",
    },
    {
        id: "f2",
        title: "The Dark Knight",
        poster: "/dark-knight-poster.png",
        rating: 9.0,
        year: 2008,
        duration: "2h 32m",
    },
    {
        id: "f3",
        title: "Interstellar",
        poster: "/interstellar-movie-poster.jpg",
        rating: 8.6,
        year: 2014,
        duration: "2h 49m",
    },
    {
        id: "f4",
        title: "The Matrix",
        poster: "/matrix-movie-poster.png",
        rating: 8.7,
        year: 1999,
        duration: "2h 16m",
    },
    {
        id: "f5",
        title: "Pulp Fiction",
        poster: "/pulp-fiction-poster.png",
        rating: 8.9,
        year: 1994,
        duration: "2h 34m",
    },
    {
        id: "f6",
        title: "Fight Club",
        poster: "/fight-club-poster.png",
        rating: 8.8,
        year: 1999,
        duration: "2h 19m",
    },
]

export const mockSerials: Movie[] = [
    {
        id: "s1",
        title: "Breaking Bad",
        poster: "/breaking-bad-inspired-poster.png",
        rating: 9.5,
        year: 2008,
        duration: "5 seasons",
    },
    {
        id: "s2",
        title: "Game of Thrones",
        poster: "/game-of-thrones-inspired-poster.png",
        rating: 9.2,
        year: 2011,
        duration: "8 seasons",
    },
    {
        id: "s3",
        title: "Stranger Things",
        poster: "/stranger-things-inspired-poster.png",
        rating: 8.7,
        year: 2016,
        duration: "4 seasons",
    },
    {
        id: "s4",
        title: "The Crown",
        poster: "/the-crown-poster.jpg",
        rating: 8.6,
        year: 2016,
        duration: "6 seasons",
    },
    {
        id: "s5",
        title: "The Mandalorian",
        poster: "/mandalorian-inspired-poster.png",
        rating: 8.7,
        year: 2019,
        duration: "3 seasons",
    },
]

export const mockCartoons: Movie[] = [
    {
        id: "c1",
        title: "Toy Story",
        poster: "/toy-story-poster.jpg",
        rating: 8.3,
        year: 1995,
        duration: "1h 21m",
    },
    {
        id: "c2",
        title: "Finding Nemo",
        poster: "/finding-nemo-poster.jpg",
        rating: 8.1,
        year: 2003,
        duration: "1h 40m",
    },
    {
        id: "c3",
        title: "The Lion King",
        poster: "/lion-king-poster.jpg",
        rating: 8.5,
        year: 1994,
        duration: "1h 28m",
    },
    {
        id: "c4",
        title: "Shrek",
        poster: "/shrek-poster.jpg",
        rating: 7.9,
        year: 2001,
        duration: "1h 30m",
    },
    {
        id: "c5",
        title: "Up",
        poster: "/up-pixar-poster.jpg",
        rating: 8.2,
        year: 2009,
        duration: "1h 36m",
    },
]

export const mockAnime: Movie[] = [
    {
        id: "a1",
        title: "Spirited Away",
        poster: "/spirited-away-poster.jpg",
        rating: 8.6,
        year: 2001,
        duration: "2h 5m",
    },
    {
        id: "a2",
        title: "Your Name",
        poster: "/your-name-anime-poster.png",
        rating: 8.4,
        year: 2016,
        duration: "1h 46m",
    },
    {
        id: "a3",
        title: "Princess Mononoke",
        poster: "/princess-mononoke-poster.jpg",
        rating: 8.4,
        year: 1997,
        duration: "2h 14m",
    },
    {
        id: "a4",
        title: "Akira",
        poster: "/akira-anime-poster.jpg",
        rating: 8.0,
        year: 1988,
        duration: "2h 4m",
    },
    {
        id: "a5",
        title: "Demon Slayer",
        poster: "/demon-slayer-poster.jpg",
        rating: 8.7,
        year: 2019,
        duration: "3 seasons",
    },
]

export const mockNovelty: Movie[] = [
    {
        id: "n1",
        title: "Dune: Part Two",
        poster: "/dune-part-two-poster.jpg",
        rating: 8.8,
        year: 2024,
        duration: "2h 46m",
    },
    {
        id: "n2",
        title: "Oppenheimer",
        poster: "/images/posters/oppenheimer-poster.png",
        rating: 8.5,
        year: 2023,
        duration: "3h 0m",
    },
    {
        id: "n3",
        title: "The Batman",
        poster: "/the-batman-2022-poster.jpg",
        rating: 7.8,
        year: 2022,
        duration: "2h 56m",
    },
    {
        id: "n4",
        title: "Everything Everywhere",
        poster: "/eeaao-poster.png",
        rating: 7.8,
        year: 2022,
        duration: "2h 19m",
    },
    {
        id: "n5",
        title: "Avatar: The Way of Water",
        poster: "/avatar-way-of-water-poster.jpg",
        rating: 7.6,
        year: 2022,
        duration: "3h 12m",
    },
]

export const mockNews: News[] = [
    {
        id: "news1",
        title: "New Sci-Fi Epic Coming This Summer",
        excerpt:
            "Get ready for the most anticipated science fiction movie of the year. Featuring stunning visuals and an all-star cast.",
        image: "/sci-fi-movie-scene.jpg",
        date: "Oct 20, 2025",
    },
    {
        id: "news2",
        title: "Classic Films Return to Theaters",
        excerpt:
            "Experience cinema history on the big screen. A collection of timeless classics will be showing throughout the month.",
        image: "/classic-cinema-theater.jpg",
        date: "Oct 18, 2025",
    },
    {
        id: "news3",
        title: "Anime Festival Announced",
        excerpt: "Join us for a celebration of Japanese animation. Three days of screenings, panels, and special guests.",
        image: "/anime-festival.jpg",
        date: "Oct 15, 2025",
    },
    {
        id: "news4",
        title: "Behind the Scenes: Making of Blockbusters",
        excerpt: "Discover the magic of filmmaking with exclusive behind-the-scenes content from this year's biggest hits.",
        image: "/movie-production-behind-scenes.jpg",
        date: "Oct 12, 2025",
    },
    {
        id: "news5",
        title: "Documentary Series Explores Cinema History",
        excerpt:
            "A new documentary series takes you through 100 years of cinema, from silent films to modern masterpieces.",
        image: "/vintage-film-camera.jpg",
        date: "Oct 10, 2025",
    },
    {
        id: "news6",
        title: "International Film Week Begins",
        excerpt: "Experience cinema from around the world. A curated selection of award-winning international films.",
        image: "/world-cinema-film-reel.jpg",
        date: "Oct 8, 2025",
    },
]

export const mockMessages: Message[] = [
    {
        id: "msg1",
        sender: "Sarah Johnson",
        subject: "Movie Recommendation",
        preview: "Hey! I just watched this amazing thriller and thought you might like it...",
        body: "Hey! I just watched this amazing thriller and thought you might like it. It's called 'The Silent Hour' and it has incredible plot twists. The cinematography is stunning and the acting is top-notch. Let me know if you watch it!",
        date: "2 hours ago",
        read: false,
    },
    {
        id: "msg2",
        sender: "Mike Chen",
        subject: "Movie Night This Weekend?",
        preview: "Are you free this Saturday? I was thinking we could have a movie marathon...",
        body: "Are you free this Saturday? I was thinking we could have a movie marathon at my place. I've got a great selection of classic films lined up. We can start around 6 PM. Let me know if you're interested!",
        date: "1 day ago",
        read: false,
    },
    {
        id: "msg3",
        sender: "Emma Wilson",
        subject: "Thanks for the suggestion!",
        preview: "I finally watched the movie you recommended last month...",
        body: "I finally watched the movie you recommended last month and it was absolutely fantastic! You were right about everything. The story was gripping from start to finish. Thanks for the great recommendation!",
        date: "3 days ago",
        read: true,
    },
    {
        id: "msg4",
        sender: "Alex Turner",
        subject: "Film Club Meeting",
        preview: "Reminder: Our monthly film club meeting is scheduled for next Tuesday...",
        body: "Reminder: Our monthly film club meeting is scheduled for next Tuesday at 7 PM. We'll be discussing this month's selection and voting on next month's film. Hope to see you there!",
        date: "5 days ago",
        read: true,
    },
]

export const mockFriends: User[] = [
    { id: "u1", name: "Sarah Johnson", email: "sarah.j@email.com" },
    { id: "u2", name: "Mike Chen", email: "mike.chen@email.com" },
    { id: "u3", name: "Emma Wilson", email: "emma.w@email.com" },
    { id: "u4", name: "Alex Turner", email: "alex.t@email.com" },
    { id: "u5", name: "Lisa Anderson", email: "lisa.a@email.com" },
]

export const mockAllUsers: User[] = [
    ...mockFriends,
    { id: "u6", name: "David Brown", email: "david.b@email.com" },
    { id: "u7", name: "Rachel Green", email: "rachel.g@email.com" },
    { id: "u8", name: "Tom Harris", email: "tom.h@email.com" },
    { id: "u9", name: "Nina Patel", email: "nina.p@email.com" },
    { id: "u10", name: "Chris Martin", email: "chris.m@email.com" },
]

export function getAllMovies(): Movie[] {
    return [...mockFilms, ...mockSerials, ...mockCartoons, ...mockAnime, ...mockNovelty]
}
