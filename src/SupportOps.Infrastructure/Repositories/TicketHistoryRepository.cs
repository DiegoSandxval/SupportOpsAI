using Microsoft.EntityFrameworkCore;
using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Domain.Entities;

namespace SupportOps.Infrastructure.Persistence.Repositories;

public sealed class TicketHistoryRepository
    : ITicketHistoryRepository
{
    private readonly SupportOpsDbContext _dbContext;

    public TicketHistoryRepository(
        SupportOpsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        TicketHistory history,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.TicketHistory.AddAsync(
            history,
            cancellationToken
        );
    }

    public async Task<IReadOnlyList<TicketHistory>> GetByTicketIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.TicketHistory
            .AsNoTracking()
            .Where(history => history.TicketId == ticketId)
            .OrderBy(history => history.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}