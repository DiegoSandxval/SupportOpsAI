using System;
using System.Collections.Generic;
using System.Text;

namespace SupportOps.Application.Auth.Login;

public sealed record LoginRequest(
    string Email,
    string Password
);