using SupportOps.Domain.Enums;

namespace SupportOps.Application.Tickets.Update;

public sealed record UpdateTicketRequest(
    TicketPriority? Priority,
    TicketCategory? Category,
    Guid? AssignedAgentId,
    TicketStatus? Status
);