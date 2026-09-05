namespace SupportOps.Application.Users.Profile;

public sealed record UpdateProfileResponse(
    Guid Id,
    string FullName,
    string Email,
    string Role
);