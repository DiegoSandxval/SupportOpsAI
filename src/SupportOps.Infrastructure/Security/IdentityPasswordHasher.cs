using Microsoft.AspNetCore.Identity;
using SupportOps.Application.Abstractions.Security;

namespace SupportOps.Infrastructure.Security;

public sealed class IdentityPasswordHasher : IPasswordHasher
{
    private static readonly object User = new();

    private readonly PasswordHasher<object> _passwordHasher = new();

    public string Hash(string password)
    {
        return _passwordHasher.HashPassword(
            User,
            password
        );
    }

    public bool Verify(
        string password,
        string passwordHash)
    {
        var result =
            _passwordHasher.VerifyHashedPassword(
                User,
                passwordHash,
                password
            );

        return result != PasswordVerificationResult.Failed;
    }
}