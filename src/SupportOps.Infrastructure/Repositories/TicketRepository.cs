using Microsoft.EntityFrameworkCore;
using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Domain.Entities;

namespace SupportOps.Infrastructure.Persistence.Repositories;

public sealed class TicketRepository : ITicketRepository
{
    private readonly SupportOpsDbContext _dbContext;

    public TicketRepository(
        SupportOpsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Ticket ticket,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Tickets.AddAsync(
            ticket,
            cancellationToken
        );
    }

    public async Task<Ticket?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tickets
            .FirstOrDefaultAsync(
                ticket => ticket.Id == id,
                cancellationToken
            );
    }

    public async Task<IReadOnlyList<Ticket>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tickets
            .AsNoTracking()
            .OrderByDescending(ticket => ticket.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Ticket>> GetByCreatedByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Tickets
            .AsNoTracking()
            .Where(ticket =>
                ticket.CreatedByUserId == userId)
            .OrderByDescending(ticket =>
                ticket.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }
}