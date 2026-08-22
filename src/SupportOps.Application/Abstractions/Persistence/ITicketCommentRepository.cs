using SupportOps.Domain.Entities;

namespace SupportOps.Application.Abstractions.Persistence;

public interface ITicketCommentRepository
{
    Task AddAsync(
        TicketComment comment,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TicketComment>> GetByTicketIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default);
}