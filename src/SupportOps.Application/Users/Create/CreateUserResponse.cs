namespace SupportOps.Application.Users.Create;

public sealed record CreateUserResponse(
    Guid Id,
    string FullName,
    string Email,
    string Role
);