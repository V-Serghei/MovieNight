"use client"

import { DropdownMenu, DropdownMenuContent, DropdownMenuItem, DropdownMenuTrigger } from "@/components/ui/dropdown-menu"
import { Button } from "@/components/ui/button"
import { ChevronDown } from "lucide-react"
import type { SortOption } from "@/lib/types"

interface SortDropdownProps {
    value: SortOption
    onChange: (value: SortOption) => void
}

const sortOptions: { value: SortOption; label: string }[] = [
    { value: "rating", label: "Rating" },
    { value: "year", label: "Year" },
    { value: "alphabet", label: "Alphabet" },
    { value: "popularity", label: "Popularity" },
    { value: "duration", label: "Duration" },
]

export function SortDropdown({ value, onChange }: SortDropdownProps) {
    const currentLabel = sortOptions.find((opt) => opt.value === value)?.label || "Sort by"

    return (
        <DropdownMenu>
            <DropdownMenuTrigger asChild>
                <Button variant="outline" className="gap-2 bg-transparent">
                    Sort by: {currentLabel}
                    <ChevronDown className="h-4 w-4" />
                </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
                {sortOptions.map((option) => (
                    <DropdownMenuItem
                        key={option.value}
                        onClick={() => onChange(option.value)}
                        className={value === option.value ? "bg-muted" : ""}
                    >
                        {option.label}
                    </DropdownMenuItem>
                ))}
            </DropdownMenuContent>
        </DropdownMenu>
    )
}
