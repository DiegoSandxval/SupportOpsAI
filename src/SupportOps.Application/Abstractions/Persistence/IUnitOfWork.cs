using System;
using System.Collections.Generic;
using System.Text;

namespace SupportOps.Application.Abstractions.Persistence;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}