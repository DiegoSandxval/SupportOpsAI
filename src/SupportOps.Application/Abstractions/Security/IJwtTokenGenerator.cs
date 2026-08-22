using SupportOps.Domain.Entities;

namespace SupportOps.Application.Abstractions.Security;

public interface IJwtTokenGenerator
{
    JwtTokenResult Generate(User user);
}

public sealed record JwtTokenResult(
    string AccessToken,
    DateTime ExpiresAtUtc
);