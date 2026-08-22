using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Application.Abstractions.Security;
using SupportOps.Application.Common.Exceptions;

namespace SupportOps.Application.Auth.Login;

public sealed class LoginHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    public LoginHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
    }

    public async Task<LoginResponse> HandleAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail =
            request.Email.Trim().ToLowerInvariant();

        var user =
            await _userRepository.GetByEmailAsync(
                normalizedEmail,
                cancellationToken
            );

        if (user is null)
        {
            throw new InvalidCredentialsException();
        }

        var passwordIsValid =
            _passwordHasher.Verify(
                request.Password,
                user.PasswordHash
            );

        if (!passwordIsValid)
        {
            throw new InvalidCredentialsException();
        }

        if (!user.IsActive)
        {
            throw new InactiveUserException();
        }

        var token =
            _jwtTokenGenerator.Generate(user);

        return new LoginResponse(
            token.AccessToken,
            token.ExpiresAtUtc,
            new LoginUserResponse(
                user.Id,
                user.GetFullName(),
                user.Email,
                user.Role.ToString()
            )
        );
    }
}