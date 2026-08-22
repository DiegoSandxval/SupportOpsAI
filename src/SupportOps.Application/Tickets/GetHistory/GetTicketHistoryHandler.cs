using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Domain.Enums;

namespace SupportOps.Application.Tickets.GetHistory;

public sealed class GetTicketHistoryHandler
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketHistoryRepository _historyRepository;

    public GetTicketHistoryHandler(
        ITicketRepository ticketRepository,
        ITicketHistoryRepository historyRepository)
    {
        _ticketRepository = ticketRepository;
        _historyRepository = historyRepository;
    }

    public async Task<IReadOnlyList<GetTicketHistoryResponse>?> HandleAsync(
        Guid ticketId,
        Guid userId,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(
            ticketId,
            cancellationToken
        );

        if (ticket is null)
        {
            return null;
        }

        if (role == UserRole.User &&
            ticket.CreatedByUserId != userId)
        {
            return null;
        }

        if (role != UserRole.User &&
            role != UserRole.Agent &&
            role != UserRole.Admin)
        {
            return null;
        }

        var history =
            await _historyRepository.GetByTicketIdAsync(
                ticketId,
                cancellationToken
            );

        return history
            .Select(item =>
                new GetTicketHistoryResponse(
                    item.Id,
                    item.TicketId,
                    item.ChangedByUserId,
                    item.Action.ToString(),
                    item.PreviousValue,
                    item.NewValue,
                    item.Description,
                    item.CreatedAtUtc
                ))
            .ToList();
    }
}