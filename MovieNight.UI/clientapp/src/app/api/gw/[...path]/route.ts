export const dynamic = 'force-dynamic';

const GATEWAY = process.env.GATEWAY_URL ?? 'http://movienight.localtest.me';

function buildTargetUrl(req: Request, pathParts?: string[]) {
    const url = new URL(req.url);
    const subPath = (pathParts ?? []).join('/');
    const qs = url.search;
    const target = `${GATEWAY}/${subPath}${qs}`;
    return target.replace(/(?<!:)\/{2,}/g, '/').replace(/^http(s?):\//, 'http$1://');
}

async function forward(req: Request, pathParts?: string[]) {
    const targetUrl = buildTargetUrl(req, pathParts);

    const method = req.method;
    const headers = new Headers(req.headers);
    ['host', 'content-length', 'connection', 'accept-encoding'].forEach(h => headers.delete(h));
    headers.set('x-forwarded-for', req.headers.get('x-forwarded-for') ?? 'MovieNight.UI');

    let body: BodyInit | undefined = undefined;
    if (!['GET', 'HEAD'].includes(method)) {
        const buf = await req.arrayBuffer();
        body = buf.byteLength ? buf : undefined;
    }

    try {
        const res = await fetch(targetUrl, { method, headers, body, cache: 'no-store' });
        const outHeaders = new Headers(res.headers);
        return new Response(res.body, { status: res.status, headers: outHeaders });
    } catch (err: any) {
        return Response.json(
            { error: 'Bad Gateway', detail: err?.message, targetUrl },
            { status: 502 }
        );
    }
}

type Params = { path?: string[] };

export async function GET(req: Request, ctx: { params: Promise<Params> }) {
    const { path } = await ctx.params;
    return forward(req, path);
}
export async function POST(req: Request, ctx: { params: Promise<Params> }) {
    const { path } = await ctx.params;
    return forward(req, path);
}
export async function PUT(req: Request, ctx: { params: Promise<Params> }) {
    const { path } = await ctx.params;
    return forward(req, path);
}
export async function PATCH(req: Request, ctx: { params: Promise<Params> }) {
    const { path } = await ctx.params;
    return forward(req, path);
}
export async function DELETE(req: Request, ctx: { params: Promise<Params> }) {
    const { path } = await ctx.params;
    return forward(req, path);
}
export async function OPTIONS(req: Request, ctx: { params: Promise<Params> }) {
    const { path } = await ctx.params;
    return forward(req, path);
}
