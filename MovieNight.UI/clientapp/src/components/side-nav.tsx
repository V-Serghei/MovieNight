"use client"

import Link from "next/link"
import { usePathname } from "next/navigation"
import { Home, Film, Mail, Users, ChevronDown } from "lucide-react"
import { Button } from "@/components/ui/button"
import { Collapsible, CollapsibleContent, CollapsibleTrigger } from "@/components/ui/collapsible"
import { cn } from "@/lib/utils"
import { useState } from "react"

const navItems = [
    { href: "/", label: "Home", icon: Home },
    {
        label: "Cinema",
        icon: Film,
        children: [
            { href: "/cinema/films", label: "Films" },
            { href: "/cinema/serials", label: "Serials" },
            { href: "/cinema/cartoons", label: "Cartoons" },
            { href: "/cinema/anime", label: "Anime" },
            { href: "/cinema/random", label: "Random Film" },
            { href: "/cinema/novelty", label: "Novelty" },
        ],
    },
    {
        label: "Message",
        icon: Mail,
        children: [
            { href: "/messages/inbox", label: "Inbox" },
            { href: "/messages/compose", label: "Compose message" },
        ],
    },
    { href: "/friends", label: "My friends", icon: Users },
]

export function SideNav() {
    const pathname = usePathname()
    const [openSections, setOpenSections] = useState<string[]>(["Cinema", "Message"])

    const toggleSection = (label: string) => {
        setOpenSections((prev) => (prev.includes(label) ? prev.filter((s) => s !== label) : [...prev, label]))
    }

    return (
        <nav className="flex flex-col h-full bg-card">
            <div className="p-6 border-b border-border">
                <h2 className="font-serif font-bold text-xl">Navigation</h2>
            </div>

            <div className="flex-1 overflow-y-auto p-4 space-y-1">
                {navItems.map((item) => {
                    if ("children" in item) {
                        const isOpen = openSections.includes(item.label)
                        return (
                            <Collapsible key={item.label} open={isOpen} onOpenChange={() => toggleSection(item.label)}>
                                <CollapsibleTrigger asChild>
                                    <Button variant="ghost" className="w-full justify-between hover:bg-muted">
                    <span className="flex items-center gap-3">
                      <item.icon className="h-5 w-5" />
                        {item.label}
                    </span>
                                        <ChevronDown className={cn("h-4 w-4 transition-transform", isOpen && "rotate-180")} />
                                    </Button>
                                </CollapsibleTrigger>
                                <CollapsibleContent className="pl-8 space-y-1 mt-1">
                                    {item.children.map((child) => (
                                        <Button
                                            key={child.href}
                                            variant="ghost"
                                            asChild
                                            className={cn(
                                                "w-full justify-start hover:bg-muted",
                                                pathname === child.href && "bg-muted text-primary",
                                            )}
                                        >
                                            <Link href={child.href}>{child.label}</Link>
                                        </Button>
                                    ))}
                                </CollapsibleContent>
                            </Collapsible>
                        )
                    }

                    return (
                        <Button
                            key={item.href}
                            variant="ghost"
                            asChild
                            className={cn("w-full justify-start hover:bg-muted", pathname === item.href && "bg-muted text-primary")}
                        >
                            <Link href={item.href}>
                                <item.icon className="mr-3 h-5 w-5" />
                                {item.label}
                            </Link>
                        </Button>
                    )
                })}
            </div>
        </nav>
    )
}
