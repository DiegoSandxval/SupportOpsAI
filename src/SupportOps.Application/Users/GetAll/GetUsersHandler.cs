using SupportOps.Application.Abstractions.Persistence;

namespace SupportOps.Application.Users.GetAll;

public sealed class GetUsersHandler
{
    private readonly IUserRepository _userRepository;

    public GetUsersHandler(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<UserListItemResponse>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var users =
            await _userRepository.GetAllAsync(
                cancellationToken
            );

        return users
            .Select(user =>
                new UserListItemResponse(
                    user.Id,
                    user.GetFullName(),
                    user.Email,
                    user.Role.ToString(),
                    user.IsActive
                ))
            .ToList();
    }
}