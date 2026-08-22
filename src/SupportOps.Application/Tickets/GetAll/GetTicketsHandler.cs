using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Domain.Enums;

namespace SupportOps.Application.Tickets.GetAll;

public sealed class GetTicketsHandler
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketsHandler(
        ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<IReadOnlyList<TicketListItemResponse>> HandleAsync(
        Guid userId,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        var tickets =
            role is UserRole.Agent or UserRole.Admin
                ? await _ticketRepository.GetAllAsync(
                    cancellationToken
                )
                : await _ticketRepository
                    .GetByCreatedByUserIdAsync(
                        userId,
                        cancellationToken
                    );

        return tickets
            .Select(ticket =>
                new TicketListItemResponse(
                    ticket.Id,
                    ticket.Title,
                    ticket.Status.ToString(),
                    ticket.Priority.ToString(),
                    ticket.Category.ToString(),
                    ticket.CreatedByUserId,
                    ticket.AssignedAgentId,
                    ticket.CreatedAtUtc
                ))
            .ToList();
    }
}