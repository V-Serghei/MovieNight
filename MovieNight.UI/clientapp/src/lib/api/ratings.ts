export type MovieRatingSummary = {
    movieId: string;
    averageRating: number | null;
    ratingsCount: number;
    userRating: number | null;
};

const BASE = "/api/gw/ratings";

export async function getMovieRating(movieId: string): Promise<MovieRatingSummary> {
    const resp = await fetch(`${BASE}/movies/${movieId}`, {
        credentials: "include",
    });
    if (!resp.ok) throw new Error("Failed to load rating");
    return await resp.json();
}

// ids: string[] -> GET /ratings/movies?ids=...
export async function getMovieRatingsBulk(ids: string[]): Promise<MovieRatingSummary[]> {
    if (ids.length === 0) return [];
    const query = encodeURIComponent(ids.join(","));
    const resp = await fetch(`${BASE}/movies?ids=${query}`, {
        credentials: "include",
    });
    if (!resp.ok) throw new Error("Failed to load ratings");
    return await resp.json();
}

// PUT /ratings/movies/{movieId}
export async function setMovieRating(movieId: string, score: number): Promise<MovieRatingSummary> {
    const resp = await fetch(`${BASE}/movies/${movieId}`, {
        method: "PUT",
        credentials: "include",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ score }),
    });
    if (!resp.ok) throw new Error("Failed to set rating");
    return await resp.json();
}

// DELETE /ratings/movies/{movieId}
export async function deleteMovieRating(movieId: string): Promise<MovieRatingSummary> {
    const resp = await fetch(`${BASE}/movies/${movieId}`, {
        method: "DELETE",
        credentials: "include",
    });
    if (!resp.ok) throw new Error("Failed to delete rating");
    return await resp.json();
}
