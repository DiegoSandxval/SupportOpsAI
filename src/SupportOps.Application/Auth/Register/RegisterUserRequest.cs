using System;
using System.Collections.Generic;
using System.Text;

namespace SupportOps.Application.Auth.Register;

public sealed record RegisterUserRequest(
    string FirstName,
    string LastName,
    string Email,
    string Password
);