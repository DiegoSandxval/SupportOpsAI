namespace SupportOps.Application.Users.Profile;

public sealed record GetProfileResponse(
    Guid Id,
    string FirstName,
    string LastName,
    string Email,
    string Role
);