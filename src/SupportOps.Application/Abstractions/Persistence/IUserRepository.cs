using SupportOps.Domain.Entities;

namespace SupportOps.Application.Abstractions.Persistence;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<User>> GetActiveAgentsAsync(
        CancellationToken cancellationToken = default
    );

    Task AddAsync(
        User user,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<User>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default
    );
    Task<IReadOnlyList<User>> GetAllAsync(
    CancellationToken cancellationToken = default
);
}

