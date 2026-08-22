using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Application.Tickets.GetTicketById;
using SupportOps.Domain.Enums;

namespace SupportOps.Application.Tickets.GetTicketById;

public sealed class GetTicketByIdHandler
{
    private readonly ITicketRepository _ticketRepository;

    public GetTicketByIdHandler(ITicketRepository ticketRepository)
    {
        _ticketRepository = ticketRepository;
    }

    public async Task<GetTicketByIdResponse?> HandleAsync(
        GetTicketByIdQuery query,
        Guid userId,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(
            query.TicketId,
            cancellationToken);

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

        return new GetTicketByIdResponse(
            ticket.Id,
            ticket.Title,
            ticket.Status.ToString(),
            ticket.Priority.ToString(),
            ticket.Category.ToString(),
            ticket.CreatedByUserId,
            ticket.AssignedAgentId,
            ticket.CreatedAtUtc,
            ticket.ResolvedAtUtc,
            ticket.ClosedAtUtc
        );
    }
}