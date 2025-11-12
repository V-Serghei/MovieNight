import { Card, CardContent, CardFooter, CardHeader } from "@/components/ui/card"
import { Button } from "@/components/ui/button"
import { Calendar } from "lucide-react"
import type { News } from "@/lib/types"

interface NewsCardProps {
    news: News
}

export function NewsCard({ news }: NewsCardProps) {
    return (
        <Card className="overflow-hidden bg-card/50 backdrop-blur border-border hover:border-primary/50 transition-colors">
            <div className="aspect-video relative overflow-hidden bg-muted">
                <img src={news.image || "/placeholder.svg"} alt={news.title} className="object-cover w-full h-full" />
            </div>
            <CardHeader>
                <div className="flex items-center gap-2 text-sm text-muted-foreground mb-2">
                    <Calendar className="h-4 w-4" />
                    {news.date}
                </div>
                <h3 className="font-serif font-bold text-xl text-balance leading-tight">{news.title}</h3>
            </CardHeader>
            <CardContent>
                <p className="text-muted-foreground line-clamp-3">{news.excerpt}</p>
            </CardContent>
            <CardFooter>
                <Button variant="outline" className="w-full hover:bg-primary hover:text-primary-foreground bg-transparent">
                    Read more
                </Button>
            </CardFooter>
        </Card>
    )
}
