using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Access.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using MovieNight.Gateway.Endpoints;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

// === HTTP clients ===
builder.Services.AddHttpClient<AccessClientHttp>(c =>
{
    c.BaseAddress = new Uri(builder.Configuration["Services:Access"] ?? "http://localhost:7003");
});
builder.Services.AddHttpClient("auth", c =>
{
    // Tokens.Service (login/refresh/logout/me)
    c.BaseAddress = new Uri(builder.Configuration["Services:Auth"] ?? "http://localhost:7010");
});
builder.Services.AddHttpClient("users", c =>
{
    // Users.Service (register, get user)
    c.BaseAddress = new Uri(builder.Configuration["Services:Users"] ?? "http://localhost:7001");
});
builder.Services.AddHttpClient("movies", c =>
{
    // пример бизнес-сервиса
    c.BaseAddress = new Uri(builder.Configuration["Services:Movies"] ?? "http://localhost:7020");
});

// === Access Proxy (Proxy pattern + cache) ===
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IAccessClient>(sp =>
{
    var inner = sp.GetRequiredService<AccessClientHttp>();
    var cache = sp.GetRequiredService<IMemoryCache>();
    return new AccessClientProxy(inner, cache, TimeSpan.FromSeconds(5));
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

app.MapGet("/health", () => Results.Ok(new { status = "ok" })).WithTags("Infra");
app.MapGet("/debug/ping", () => "pong").WithTags("Infra");
app.MapGet("/api/v1", () => "Gateway v1").WithTags("Infra");

app.UseAuthentication();
app.UseAuthorization();

string[] aclSkipPrefixes =
{
    "/auth",
    "/openapi", "/scalar",
    "/health", "/debug"
};

app.Use(async (ctx, next) =>
{
    var path = ctx.Request.Path.Value ?? "/";
    if (aclSkipPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
    {
        await next(); return;
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

    var role = user.FindFirstValue(ClaimTypes.Role) ?? "user";
    var resource = path;
    var method   = ctx.Request.Method;

    var acl = ctx.RequestServices.GetRequiredService<IAccessClient>();
    var allowed = await acl.IsAllowedAsync(userId, resource, method, null, null, ctx.RequestAborted);

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

app.MapAuthProxy();

app.MapMoviesProxy();

app.Run();
