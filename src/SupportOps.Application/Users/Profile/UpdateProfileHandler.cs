using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Application.Common.Exceptions;

namespace SupportOps.Application.Users.Profile;

public sealed class UpdateProfileHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateProfileHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository =
            userRepository;

        _unitOfWork =
            unitOfWork;
    }

    public async Task<UpdateProfileResponse> HandleAsync(
        Guid userId,
        UpdateProfileRequest request,
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

        var normalizedEmail =
            request.Email
                .Trim()
                .ToLowerInvariant();

        if (
            normalizedEmail !=
            user.Email
        )
        {
            var emailExists =
                await _userRepository.ExistsByEmailAsync(
                    normalizedEmail,
                    cancellationToken
                );

            if (emailExists)
            {
                throw new DuplicateEmailException(
                    normalizedEmail
                );
            }
        }

        user.ChangeName(
            request.FirstName,
            request.LastName
        );

        user.ChangeEmail(
            normalizedEmail
        );

        await _unitOfWork.SaveChangesAsync(
            cancellationToken
        );

        return new UpdateProfileResponse(
            user.Id,
            user.GetFullName(),
            user.Email,
            user.Role.ToString()
        );
    }
}