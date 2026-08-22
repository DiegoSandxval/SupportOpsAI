using SupportOps.Domain.Enums;

namespace SupportOps.Application.Auth.Register;

public sealed record RegisterUserResponse(
    Guid Id,
    string FullName,
    string Email,
    string Role
);