using Microsoft.EntityFrameworkCore;
using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Domain.Entities;

namespace SupportOps.Infrastructure.Persistence.Repositories;

public sealed class TicketCommentRepository
    : ITicketCommentRepository
{
    private readonly SupportOpsDbContext _dbContext;

    public TicketCommentRepository(
        SupportOpsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        TicketComment comment,
        CancellationToken cancellationToken = default)
    {
        await _dbContext
            .Set<TicketComment>()
            .AddAsync(
                comment,
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<TicketComment>> GetByTicketIdAsync(
        Guid ticketId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext
            .Set<TicketComment>()
            .AsNoTracking()
            .Where(x => x.TicketId == ticketId)
            .OrderBy(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}