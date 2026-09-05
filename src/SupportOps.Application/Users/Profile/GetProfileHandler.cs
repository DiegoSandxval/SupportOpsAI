using SupportOps.Application.Abstractions.Persistence;

namespace SupportOps.Application.Users.Profile;

public sealed class GetProfileHandler
{
    private readonly IUserRepository _userRepository;

    public GetProfileHandler(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<GetProfileResponse?> HandleAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var user =
            await _userRepository.GetByIdAsync(
                userId,
                cancellationToken
            );

        if (user is null)
        {
            return null;
        }

        return new GetProfileResponse(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.Role.ToString()
        );
    }
}