using Microsoft.EntityFrameworkCore;
using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Domain.Entities;
using SupportOps.Domain.Enums;

namespace SupportOps.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly SupportOpsDbContext _dbContext;

    public UserRepository(SupportOpsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(
                user => user.Id == id,
                cancellationToken
            );
    }

    public async Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail =
            email.Trim().ToLowerInvariant();

        return await _dbContext.Users
            .FirstOrDefaultAsync(
                user => user.Email == normalizedEmail,
                cancellationToken
            );
    }

    public async Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail =
            email.Trim().ToLowerInvariant();

        return await _dbContext.Users
            .AnyAsync(
                user => user.Email == normalizedEmail,
                cancellationToken
            );
    }

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(
            user,
            cancellationToken
        );
    }

    public async Task<IReadOnlyList<User>> GetActiveAgentsAsync(
    CancellationToken cancellationToken = default)
    {
        return await _dbContext.Users
            .AsNoTracking()
            .Where(user =>
                user.Role == UserRole.Agent &&
                user.IsActive
            )
            .OrderBy(user => user.FirstName)
            .ThenBy(user => user.LastName)
            .ToListAsync(cancellationToken);
    }
    public async Task<IReadOnlyList<User>> GetByIdsAsync(
    IReadOnlyCollection<Guid> ids,
    CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await _dbContext.Users
            .AsNoTracking()
            .Where(user =>
                ids.Contains(user.Id)
            )
            .ToListAsync(
                cancellationToken
            );
    }
}