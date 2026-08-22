using System;
using System.Collections.Generic;
using System.Text;
using SupportOps.Domain.Entities;

namespace SupportOps.Application.Abstractions.Persistence;

public interface ITicketRepository
{
    Task AddAsync(
        Ticket ticket,
        CancellationToken cancellationToken = default);

    Task<Ticket?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ticket>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Ticket>> GetByCreatedByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}