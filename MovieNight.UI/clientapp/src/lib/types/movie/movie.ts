import type { MovieCategory } from "./movieCategory";

export type UIMovie = {
    id: string;
    title: string;
    category: MovieCategory | string;
    posterImage: string;
    quote: string;
    rating: number;
    year: number;
    description: string;
    productionYear: string; // или Date/ISO, зависит от API
    productionYearS: string;
    country: string;
    director: string;
    duration: string; // или ISO
    certificate: string;
    productionCompany: string;
    budget: string;
    genre: string[];
};
