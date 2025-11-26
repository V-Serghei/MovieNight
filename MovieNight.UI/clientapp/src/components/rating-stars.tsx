// src/components/rating-stars.tsx
"use client";

import { Star } from "lucide-react";
import clsx from "clsx";

type RatingStarsProps = {
    max?: number;                    
    value: number | null;            
    onChange?: (score: number) => void;
    size?: "sm" | "md";
};

export function RatingStars({
                                max = 10,
                                value,
                                onChange,
                                size = "md",
                            }: RatingStarsProps) {
    const handleClick = (n: number) => {
        if (!onChange) return;
        onChange(n);
    };

    const starSize = size === "sm" ? "h-4 w-4" : "h-5 w-5";

    return (
        <div className="flex items-center gap-1">
            {Array.from({ length: max }, (_, i) => {
                const n = i + 1;
                const active = value !== null && n <= value;
                return (
                    <button
                        key={n}
                        type="button"
                        onClick={() => handleClick(n)}
                        className={clsx(
                            "p-0.5",
                            onChange && "cursor-pointer",
                            !onChange && "cursor-default",
                        )}
                    >
                        <Star
                            className={clsx(
                                starSize,
                                active
                                    ? "fill-yellow-400 text-yellow-400"
                                    : "text-muted-foreground",
                            )}
                        />
                    </button>
                );
            })}
        </div>
    );
}
