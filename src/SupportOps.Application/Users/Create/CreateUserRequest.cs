using SupportOps.Domain.Enums;

namespace SupportOps.Application.Users.Create;

public sealed record CreateUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    UserRole Role
);