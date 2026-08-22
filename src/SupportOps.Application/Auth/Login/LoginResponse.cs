using System;
using System.Collections.Generic;
using System.Text;

namespace SupportOps.Application.Auth.Login;

public sealed record LoginResponse(
    string AccessToken,
    DateTime ExpiresAtUtc,
    LoginUserResponse User
);

public sealed record LoginUserResponse(
    Guid Id,
    string FullName,
    string Email,
    string Role
);