namespace SupportOps.Application.Users.Profile;

public sealed record UpdateProfileRequest(
    string FirstName,
    string LastName,
    string Email
);