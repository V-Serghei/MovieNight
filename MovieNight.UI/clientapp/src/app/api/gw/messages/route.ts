// app/api/gw/messages/compose/route.ts
import { NextRequest, NextResponse } from "next/server"

export async function POST(req: NextRequest) {
    const baseUrl = process.env.GATEWAY_URL ?? "http://localhost:7020"

    // читаем тело запроса как есть и просто прокидываем дальше
    const body = await req.text()

    const resp = await fetch(`${baseUrl}/messages/compose`, {
        method: "POST",
        headers: {
            "Content-Type": req.headers.get("content-type") ?? "application/json",
            // если у тебя авторизация через cookie — пробрасываем
            Cookie: req.headers.get("cookie") ?? "",
            Authorization: req.headers.get("authorization") ?? "",
        },
        body,
    })

    // Проксируем ответ как есть
    const respBody = await resp.arrayBuffer()
    const headers = new Headers()

    resp.headers.forEach((value, key) => {
        // убираем transfer-encoding, чтобы не ломать ответ
        if (key.toLowerCase() !== "transfer-encoding") {
            headers.set(key, value)
        }
    })

    return new NextResponse(respBody, {
        status: resp.status,
        statusText: resp.statusText,
        headers,
    })
}
