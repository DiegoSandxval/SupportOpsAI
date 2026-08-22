using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Application.Abstractions.Security;
using SupportOps.Application.Common.Exceptions;
using SupportOps.Domain.Entities;
using SupportOps.Domain.Enums;

namespace SupportOps.Application.Auth.Register;

public sealed class RegisterUserHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public RegisterUserHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterUserResponse> HandleAsync(
        RegisterUserRequest request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail =
            request.Email.Trim().ToLowerInvariant();

        var emailExists =
            await _userRepository.ExistsByEmailAsync(
                normalizedEmail,
                cancellationToken
            );

        if (emailExists)
        {
            throw new DuplicateEmailException(normalizedEmail);
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ArgumentException(
                "Password is required.",
                nameof(request.Password)
            );
        }

        if (request.Password.Length < 8)
        {
            throw new ArgumentException(
                "Password must contain at least 8 characters.",
                nameof(request.Password)
            );
        }

        var passwordHash =
            _passwordHasher.Hash(request.Password);

        var user = new User(
            request.FirstName,
            request.LastName,
            normalizedEmail,
            passwordHash,
            UserRole.User
        );

        await _userRepository.AddAsync(
            user,
            cancellationToken
        );

        await _unitOfWork.SaveChangesAsync(
            cancellationToken
        );

        return new RegisterUserResponse(
            user.Id,
            user.GetFullName(),
            user.Email,
            user.Role.ToString()
        );
    }
}