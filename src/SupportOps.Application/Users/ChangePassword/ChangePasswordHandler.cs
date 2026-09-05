using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Application.Abstractions.Security;

namespace SupportOps.Application.Users.ChangePassword;

public sealed class ChangePasswordHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUnitOfWork _unitOfWork;

    public ChangePasswordHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IUnitOfWork unitOfWork)
    {
        _userRepository =
            userRepository;

        _passwordHasher =
            passwordHasher;

        _unitOfWork =
            unitOfWork;
    }

    public async Task HandleAsync(
        Guid userId,
        ChangePasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user =
            await _userRepository.GetByIdAsync(
                userId,
                cancellationToken
            );

        if (user is null)
        {
            throw new KeyNotFoundException(
                "User was not found."
            );
        }

        var currentPasswordIsValid =
            _passwordHasher.Verify(
                request.CurrentPassword,
                user.PasswordHash
            );

        if (!currentPasswordIsValid)
        {
            throw new ArgumentException(
                "Current password is incorrect."
            );
        }

        if (
            string.IsNullOrWhiteSpace(
                request.NewPassword
            )
        )
        {
            throw new ArgumentException(
                "New password is required."
            );
        }

        if (
            request.NewPassword.Length < 8
        )
        {
            throw new ArgumentException(
                "New password must contain at least 8 characters."
            );
        }

        var newPasswordHash =
            _passwordHasher.Hash(
                request.NewPassword
            );

        user.ChangePasswordHash(
            newPasswordHash
        );

        await _unitOfWork.SaveChangesAsync(
            cancellationToken
        );
    }
}