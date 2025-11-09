using System.Net.Http.Headers;

namespace MovieNight.Gateway.Endpoints;

public static class CommonProxy
{
    public static HttpRequestMessage BuildOutgoingMessage(HttpContext ctx, string relativePath)
    {
        var targetUri = new Uri(relativePath + ctx.Request.QueryString, UriKind.Relative);
        var msg = new HttpRequestMessage(new HttpMethod(ctx.Request.Method), targetUri);

        if (ctx.Request.ContentLength > 0 || ctx.Request.Headers.ContainsKey("Transfer-Encoding"))
        {
            msg.Content = new StreamContent(ctx.Request.Body);
            foreach (var h in ctx.Request.Headers)
            {
                if (h.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
                    msg.Content.Headers.TryAddWithoutValidation(h.Key, h.Value.ToArray());
            }
        }

        foreach (var h in ctx.Request.Headers)
        {
            if (h.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)) continue;
            if (h.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase)) continue;
            msg.Headers.TryAddWithoutValidation(h.Key, h.Value.ToArray());
        }


        return msg;
    }

    public static async Task CopyBack(HttpContext ctx, HttpResponseMessage resp, CancellationToken ct)
    {
        ctx.Response.StatusCode = (int)resp.StatusCode;
        foreach (var h in resp.Headers)
            ctx.Response.Headers[h.Key] = h.Value.ToArray();
        foreach (var h in resp.Content.Headers)
            ctx.Response.Headers[h.Key] = h.Value.ToArray();
        ctx.Response.Headers.Remove("transfer-encoding");

        await resp.Content.CopyToAsync(ctx.Response.Body, ct);
    }
}