using System.Net;
using System.Net.Http;
using System.Text;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MovieNight.Gateway.Endpoints;
using Xunit;

namespace Gateway.Tests.Endpoints;

public class CommonProxyTests
{
    [Fact]
    public void BuildOutgoingMessage_Copies_Method_Path_Query_And_Headers_NoBody()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "GET";
        ctx.Request.Path   = "/auth/me";
        ctx.Request.QueryString = new QueryString("?foo=bar&x=1");

        ctx.Request.Headers["Authorization"] = "Bearer token";
        ctx.Request.Headers["X-Custom"]      = "value";
        ctx.Request.Headers["Host"]          = "example.com";
        ctx.Request.Headers["Content-Type"]  = "application/json";

        var msg = CommonProxy.BuildOutgoingMessage(ctx, "/internal/auth/me");

        msg.Method.Method.Should().Be("GET");
        msg.RequestUri!.ToString().Should().Be("/internal/auth/me?foo=bar&x=1");

        msg.Headers.Any(h => h.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)).Should().BeFalse();

        msg.Headers.Any(h => h.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
           .Should().BeFalse();

        msg.Headers.Authorization!.ToString().Should().Be("Bearer token");
        msg.Headers.TryGetValues("X-Custom", out var custom).Should().BeTrue();
        custom.Should().Contain("value");

        msg.Content.Should().BeNull();
    }

    [Fact]
    public async Task BuildOutgoingMessage_WithBody_Copies_Content_And_ContentHeaders()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "POST";
        ctx.Request.Path   = "/auth/login";
        ctx.Request.QueryString = new QueryString("?remember=true");

        var jsonBody = """{"user":"test","password":"secret"}""";
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonBody));
        ms.Position = 0;
        ctx.Request.Body          = ms;
        ctx.Request.ContentLength = ms.Length;

        ctx.Request.Headers["Content-Type"]   = "application/json; charset=utf-8";
        ctx.Request.Headers["Content-Length"] = ms.Length.ToString();
        ctx.Request.Headers["X-Trace"]        = "trace-123";

        var msg = CommonProxy.BuildOutgoingMessage(ctx, "/auth/login");

        msg.Method.Method.Should().Be("POST");
        msg.RequestUri!.ToString().Should().Be("/auth/login?remember=true");
        
        msg.Content.Should().NotBeNull();
        var sentBody = await msg.Content!.ReadAsStringAsync();
        sentBody.Should().Be(jsonBody);

        msg.Content.Headers.ContentType!.ToString().Should().StartWith("application/json");
        msg.Content.Headers.ContentLength.Should().Be(ms.Length);

        msg.Headers.Any(h => h.Key.StartsWith("Content-", StringComparison.OrdinalIgnoreCase))
           .Should().BeFalse();

        msg.Headers.TryGetValues("X-Trace", out var trace).Should().BeTrue();
        trace.Should().Contain("trace-123");
    }

    [Fact]
    public async Task BuildOutgoingMessage_UsesTransferEncoding_WhenNoContentLength()
    {
        var ctx = new DefaultHttpContext();
        ctx.Request.Method = "PUT";
        ctx.Request.Path   = "/auth/refresh";

        var jsonBody = """{"refreshToken":"abc"}""";
        var ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonBody));
        ms.Position = 0;
        ctx.Request.Body = ms;
        ctx.Request.Headers["Transfer-Encoding"] = "chunked";
        ctx.Request.Headers["Content-Type"]      = "application/json";
        
        var msg = CommonProxy.BuildOutgoingMessage(ctx, "/auth/refresh");
        
        msg.Content.Should().NotBeNull();
        var sentBody = await msg.Content!.ReadAsStringAsync();
        sentBody.Should().Be(jsonBody);

        msg.Content.Headers.ContentType!.ToString().Should().StartWith("application/json");
    }

    [Fact]
    public async Task CopyBack_Copies_Status_Headers_And_Body()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();

        var resp = new HttpResponseMessage(HttpStatusCode.Accepted);
        resp.Headers.Add("X-Global", "g-1");
        resp.Content = new StringContent("hello world", Encoding.UTF8, "text/plain");
        resp.Content.Headers.Add("X-Content", "c-1");
        resp.Headers.TransferEncodingChunked = true;
        
        await CommonProxy.CopyBack(ctx, resp, CancellationToken.None);
        
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status202Accepted);
        ctx.Response.Headers["X-Global"].Should().Contain("g-1");
        ctx.Response.Headers["X-Content"].Should().Contain("c-1");

        ctx.Response.Headers.ContainsKey("transfer-encoding").Should().BeFalse();

        ctx.Response.Body.Position = 0;
        var body = await new StreamReader(ctx.Response.Body).ReadToEndAsync();
        body.Should().Be("hello world");
        ctx.Response.ContentType.Should().StartWith("text/plain");
    }
}
