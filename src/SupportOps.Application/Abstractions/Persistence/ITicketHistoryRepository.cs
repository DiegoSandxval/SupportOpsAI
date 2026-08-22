using SupportOps.Domain.Entities;

namespace SupportOps.Application.Abstractions.Persistence;

public interface ITicketHistoryRepository
{
    Task AddAsync(
        TicketHistory history,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketHistory>> GetByTicketIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);
}