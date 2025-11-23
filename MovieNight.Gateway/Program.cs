using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using MovieNight.Gateway.Endpoints;
using MovieNight.Gateway.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
// === HTTP clients ===
builder.Services.AddHttpClient("users", (sp, c) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var url = cfg["Services:Users"] ?? throw new InvalidOperationException("Services:users not configured");
    c.BaseAddress = new Uri(url, UriKind.Absolute);
});

builder.Services.AddHttpClient("auth", (sp, c) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var url = cfg["Services:Auth"] ?? throw new InvalidOperationException("Services:auth not configured");
    c.BaseAddress = new Uri(url, UriKind.Absolute);
});

builder.Services.AddHttpClient("access", (sp, c) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var url = cfg["Services:Access"] ?? throw new InvalidOperationException("Services:access not configured");
    c.BaseAddress = new Uri(url, UriKind.Absolute);
});
builder.Services.AddHttpClient("movies", c =>
{
    // пример бизнес-сервиса
    c.BaseAddress = new Uri(builder.Configuration["Services:Movies"] ?? "http://localhost:7020");
});

builder.Services.AddHttpClient("media", (sp, c) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var url = cfg["Services:Media"] ?? throw new InvalidOperationException("Services:media not configured");
    c.BaseAddress = new Uri(url, UriKind.Absolute);
});
builder.Services.AddHttpClient("people", (sp, c) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var url = cfg["Services:People"] ?? throw new InvalidOperationException("Services:People not configured");
    c.BaseAddress = new Uri(url, UriKind.Absolute);
});


// === Access Proxy (Proxy pattern + cache) ===
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient<IAclClient, AclClientHttp>((sp, c) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    var url = cfg["Services:Access"] 
              ?? throw new InvalidOperationException("Services:Access not configured");
    c.BaseAddress = new Uri(url, UriKind.Absolute);
});

var issuer   = builder.Configuration["AUTH_JWT_ISSUER"]   ?? "MovieNight.Auth";
var audience = builder.Configuration["AUTH_JWT_AUDIENCE"] ?? "MovieNight.Client";
var secret   = builder.Configuration["AUTH_JWT_SECRET"]   ?? "dev-super-secret-change-me-but-32+bytes";

byte[] keyBytes = secret.StartsWith("base64:", StringComparison.OrdinalIgnoreCase)
    ? Convert.FromBase64String(secret["base64:".Length..])
    : Encoding.UTF8.GetBytes(secret);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, ValidIssuer = issuer,
            ValidateAudience = true, ValidAudience = audience,
            ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateLifetime = true, ClockSkew = TimeSpan.FromSeconds(30)
        };
        o.Events = new JwtBearerEvents
        {
            OnMessageReceived = ctx =>
            {
                if (string.IsNullOrEmpty(ctx.Token))
                    ctx.Token = ctx.Request.Cookies["access_token"];
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// OpenAPI/Scalar
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(o =>
    {
        o.Title = "MovieNight Gateway API";
        o.WithTheme(ScalarTheme.Mars);
    }).WithDisplayName("API Docs");
}



app.UseAuthentication();
app.UseAuthorization();

string[] aclSkipPrefixes =
{
    "/auth",
    "/openapi", "/scalar",
    "/health", "/debug", "/cinema/films",
    "/movies/","/movies", "/users/me", "/access","/_internal/access", "/_internal", "/media",
    "/people" ,  "/people/" 
};

app.Use(async (HttpContext ctx, Func<Task> next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/_internal"))
    {
        await next();
        return;
    }

    var path = ctx.Request.Path.Value ?? "/";
    if (aclSkipPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
    {
        await next();
        return;
    }

    var user = ctx.User;
    var idStr = user.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? user.FindFirstValue(JwtRegisteredClaimNames.Sub);

    if (string.IsNullOrWhiteSpace(idStr) || !Guid.TryParse(idStr, out var userId))
    {
        ctx.Response.StatusCode = 401;
        await ctx.Response.WriteAsJsonAsync(new { error = "Unauthorized" });
        return;
    }

    var role = user.FindFirstValue(ClaimTypes.Role) ?? "admin";
    var resource = path;
    var method   = ctx.Request.Method;

    var acl = ctx.RequestServices.GetRequiredService<IAclClient>();
    var allowed = await acl.IsAllowedAsync(userId, resource, method, ctx.RequestAborted);

    if (!allowed)
    {
        ctx.Response.StatusCode = 403;
        await ctx.Response.WriteAsJsonAsync(new { error = "Forbidden" });
        return;
    }

    ctx.Request.Headers["X-UserId"] = userId.ToString();
    ctx.Request.Headers["X-UserRole"] = role;

    await next();
});

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).WithTags("Infra");
app.MapGet("/debug/ping", () => "pong").WithTags("Infra");
app.MapGet("/api/v1", () => "Gateway v1").WithTags("Infra");

app.MapInternalUsersProxy();
app.MapInternalAccessProxy();
app.MapAuthProxy();
app.MapMoviesProxy();
app.MapUsersPublic();
app.MapMediaProxy();
app.MapPeopleProxy();

app.Run();
