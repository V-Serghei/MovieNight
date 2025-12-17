using System;
using System.Linq;
using System.Security.Claims;
using Auth.Core.Security;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Auth.Tests;

public class JwtTokenServiceTests
{
    private JwtTokenService CreateService(string secret = "dev-super-secret-change-me-but-32+bytes")
    {
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("AUTH_JWT_SECRET", secret),
                new KeyValuePair<string, string?>("AUTH_JWT_ISSUER", "TestIssuer"),
                new KeyValuePair<string, string?>("AUTH_JWT_AUDIENCE", "TestAudience")
            })
            .Build();

        return new JwtTokenService(cfg);
    }

    [Fact]
    public void Constructor_ShouldThrow_WhenSecretTooShort()
    {
        Action act = () => CreateService("short");
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*>= 32 bytes*");
    }

    [Fact]
    public void CreateAccessToken_ShouldContainRequiredClaims()
    {
        var svc = CreateService();
        var userId = Guid.NewGuid();

        var (token, exp) = svc.CreateAccessToken(
            userId,
            "u@test.com",
            "user",
            displayName: "Tester"
        );

        token.Should().NotBeNullOrWhiteSpace();
        exp.Should().BeAfter(DateTimeOffset.UtcNow);

        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var jwt = handler.ReadJwtToken(token);

        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "u@test.com");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Role && c.Value == "user");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.Name && c.Value == "Tester");
        jwt.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier && c.Value == userId.ToString());
    }

    [Fact]
    public void CreateAccessToken_ShouldAllowLifetimeOverride()
    {
        var svc = CreateService();
        var now = DateTimeOffset.UtcNow;

        var (_, exp) = svc.CreateAccessToken(
            Guid.NewGuid(),
            "u@test.com",
            "user",
            lifetime: TimeSpan.FromHours(10)
        );

        exp.Should().BeCloseTo(now.AddHours(10), TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void GetValidationParameters_ShouldReturnConfiguredIssuerAndAudience()
    {
        var svc = CreateService();

        var p = svc.GetValidationParameters();

        p.ValidIssuer.Should().Be("TestIssuer");
        p.ValidAudience.Should().Be("TestAudience");
        p.ValidateLifetime.Should().BeTrue();
        p.IssuerSigningKey.Should().NotBeNull();
    }
}
