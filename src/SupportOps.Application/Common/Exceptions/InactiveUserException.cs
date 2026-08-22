using System;
using System.Collections.Generic;
using System.Text;

namespace SupportOps.Application.Common.Exceptions;

public sealed class InactiveUserException : Exception
{
    public InactiveUserException()
        : base("This user account is inactive.")
    {
    }
}