using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Domain.Enums;

namespace SupportOps.Application.Tickets.GetHistory;

public sealed class GetTicketHistoryHandler
{
    private readonly ITicketRepository
        _ticketRepository;

    private readonly ITicketHistoryRepository
        _historyRepository;

    private readonly IUserRepository
        _userRepository;

    public GetTicketHistoryHandler(
        ITicketRepository ticketRepository,
        ITicketHistoryRepository historyRepository,
        IUserRepository userRepository)
    {
        _ticketRepository =
            ticketRepository;

        _historyRepository =
            historyRepository;

        _userRepository =
            userRepository;
    }

    public async Task<
        IReadOnlyList<GetTicketHistoryResponse>?
    > HandleAsync(
        Guid ticketId,
        Guid userId,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        var ticket =
            await _ticketRepository
                .GetByIdAsync(
                    ticketId,
                    cancellationToken
                );

        if (ticket is null)
        {
            return null;
        }

        if (
            role == UserRole.User &&
            ticket.CreatedByUserId != userId
        )
        {
            return null;
        }

        if (
            role != UserRole.User &&
            role != UserRole.Agent &&
            role != UserRole.Admin
        )
        {
            return null;
        }

        var history =
            await _historyRepository
                .GetByTicketIdAsync(
                    ticketId,
                    cancellationToken
                );

        var userIds =
            history
                .Select(
                    item =>
                        item.ChangedByUserId
                )
                .Distinct()
                .ToArray();

        var users =
            await _userRepository
                .GetByIdsAsync(
                    userIds,
                    cancellationToken
                );

        var userNames =
            users.ToDictionary(
                user => user.Id,
                user =>
                    user.GetFullName()
            );

        return history
            .Select(item =>
            {
                var userFullName =
                    userNames.TryGetValue(
                        item.ChangedByUserId,
                        out var name
                    )
                        ? name
                        : "Unknown User";

                return new GetTicketHistoryResponse(
                    item.Id,
                    item.TicketId,
                    item.ChangedByUserId,
                    userFullName,
                    item.Action.ToString(),
                    item.PreviousValue,
                    item.NewValue,
                    item.Description,
                    item.CreatedAtUtc
                );
            })
            .ToList();
    }
}