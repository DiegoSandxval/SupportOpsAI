namespace SupportOps.Application.Users.GetAll;

public sealed record UserListItemResponse(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    bool IsActive
);