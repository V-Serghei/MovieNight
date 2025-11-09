using System.Security.Claims;
using System.Security.Cryptography;
using Auth.API.DTO;
using Auth.Core.Security;
using Auth.Domain.Entities;
using Auth.Domain.Repository.Tokens;
using Auth.Infrastructure.Clients;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes, bool secureCookies)
    {
        var g = routes.MapGroup("/auth").WithTags("Auth");

        g.MapPost("/login", Login);        
        g.MapPost("/refresh", Refresh);
        g.MapPost("/logout", Logout);
        g.MapGet("/me", Me).RequireAuthorization();

        return routes;

        async Task<IResult> Login(HttpContext http, [FromBody] LoginDto dto, UsersClient users, IRefreshTokensRepository rtRepo, JwtTokenService jwt, CancellationToken ct)
        {
            var verify = await users.VerifyAsync(dto.Email, dto.Password, ct);
            if (!verify.ok || verify.user is null) return Results.Unauthorized();

            var user = verify.user;
            var (access, accessExp) = jwt.CreateAccessToken(user.Id, user.Email, user.Role ?? "user", TimeSpan.FromMinutes(30));
            http.Response.Cookies.Append("access_token", access, AccessCookie(accessExp, secureCookies));

            var rawRefresh  = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var refreshHash = HashToken.HashTokenEncoding(rawRefresh);
            var refreshExp  = DateTimeOffset.UtcNow.AddDays(dto.RememberMe ? 30 : 14);

            await rtRepo.AddAsync(new RefreshToken { UserId = user.Id, Token = refreshHash, ExpiresAt = refreshExp }, ct);
            await rtRepo.SaveChangesAsync(ct);

            http.Response.Cookies.Append("refresh_token", rawRefresh, RefreshCookie(refreshExp, secureCookies));
            return Results.Ok(new { user });
        }

        async Task<IResult> Refresh(HttpContext http, JwtTokenService jwt, IRefreshTokensRepository rtRepo, UsersClient users, CancellationToken ct)
        {
            var raw = http.Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(raw)) return Results.Unauthorized();

            var hash = HashToken.HashTokenEncoding(raw);
            var rt = await rtRepo.FindByHashAsync(hash, ct);
            if (rt is null || rt.Revoked || rt.ExpiresAt <= DateTimeOffset.UtcNow)
                return Results.Unauthorized();

            var user = await users.GetUser(rt.UserId, ct);
            if (user is null || !user.IsActive) return Results.Unauthorized();

            await rtRepo.RevokeAsync(rt.Id, ct); // rotate

            var newRaw  = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            var newHash = HashToken.HashTokenEncoding(newRaw);
            var newExp  = DateTimeOffset.UtcNow.AddDays(14);
            await rtRepo.AddAsync(new RefreshToken { UserId = user.Id, Token = newHash, ExpiresAt = newExp }, ct);
            await rtRepo.SaveChangesAsync(ct);

            var (access, accessExp) = jwt.CreateAccessToken(user.Id, user.Email!, user.Role ?? "user", TimeSpan.FromMinutes(30));
            http.Response.Cookies.Append("access_token", access,  AccessCookie(accessExp, secureCookies));
            http.Response.Cookies.Append("refresh_token", newRaw, RefreshCookie(newExp,   secureCookies));
            return Results.NoContent();
        }

        async Task<IResult> Logout(HttpContext http, IRefreshTokensRepository rtRepo, CancellationToken ct)
        {
            var raw = http.Request.Cookies["refresh_token"];
            if (!string.IsNullOrEmpty(raw))
            {
                var hash = HashToken.HashTokenEncoding(raw);
                var rt = await rtRepo.FindByHashAsync(hash, ct);
                if (rt is not null) await rtRepo.RevokeAsync(rt.Id, ct);
                await rtRepo.SaveChangesAsync(ct);
            }
            http.Response.Cookies.Delete("access_token",  new CookieOptions { Path = "/" });
            http.Response.Cookies.Delete("refresh_token", new CookieOptions { Path = "/" });
            return Results.NoContent();
        }

        IResult Me(ClaimsPrincipal principal)
        {
            var id    = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
            var email = principal.FindFirstValue(ClaimTypes.Email);
            var role  = principal.FindFirstValue(ClaimTypes.Role) ?? "user";
            return Results.Ok(new { user = new { id, email, role } });
        }

        static CookieOptions AccessCookie(DateTimeOffset exp, bool secure) => new()
        { HttpOnly = true, SameSite = SameSiteMode.Lax, Secure = secure, Expires = exp.UtcDateTime, Path = "/" };
        static CookieOptions RefreshCookie(DateTimeOffset exp, bool secure) => new()
        { HttpOnly = true, SameSite = SameSiteMode.Lax, Secure = secure, Expires = exp.UtcDateTime, Path = "/" };
    }
}
