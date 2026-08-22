using System;
using System.Collections.Generic;
using System.Text;

namespace SupportOps.Application.Common.Exceptions;

public sealed class DuplicateEmailException : Exception
{
    public DuplicateEmailException(string email)
        : base($"A user with the email '{email}' already exists.")
    {
    }
}