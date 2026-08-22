using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using SupportOps.Application.Abstractions.Security;
using SupportOps.Domain.Entities;

namespace SupportOps.Infrastructure.Security;

public sealed class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;

    public JwtTokenGenerator(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public JwtTokenResult Generate(User user)
    {
        var issuer =
            _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "JWT issuer is not configured."
            );

        var audience =
            _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "JWT audience is not configured."
            );

        var key =
            _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException(
                "JWT key is not configured."
            );

        var expirationMinutes =
            _configuration.GetValue<int>(
                "Jwt:ExpirationMinutes"
            );

        var now = DateTime.UtcNow;

        var expiresAtUtc =
            now.AddMinutes(expirationMinutes);

        var signingKey =
            new SymmetricSecurityKey(
                Convert.FromBase64String(key)
            );

        var signingCredentials =
            new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256
            );

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id.ToString()
            ),

            new Claim(
                ClaimTypes.Name,
                user.GetFullName()
            ),

            new Claim(
                ClaimTypes.Email,
                user.Email
            ),

            new Claim(
                ClaimTypes.Role,
                user.Role.ToString()
            ),

            new Claim(
                "jti",
                Guid.NewGuid().ToString()
            )
        };

        var tokenDescriptor =
            new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),

                Issuer = issuer,

                Audience = audience,

                IssuedAt = now,

                NotBefore = now,

                Expires = expiresAtUtc,

                SigningCredentials =
                    signingCredentials
            };

        var tokenHandler =
            new JsonWebTokenHandler();

        var accessToken =
            tokenHandler.CreateToken(
                tokenDescriptor
            );

        return new JwtTokenResult(
            accessToken,
            expiresAtUtc
        );
    }
}