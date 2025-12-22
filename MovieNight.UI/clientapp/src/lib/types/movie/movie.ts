import type { MovieCategory } from "./movieCategory";

export type UIMovie = {
    id: string;
    title: string;
    year: number;
    duration: string;
    posterImage: string;
    rating: number;              
    userRating?: number | null;  
};
