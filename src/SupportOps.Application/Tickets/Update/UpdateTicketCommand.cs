using SupportOps.Domain.Enums;

namespace SupportOps.Application.Tickets.Update;

public sealed record UpdateTicketCommand(
    Guid TicketId,
    TicketPriority? Priority,
    TicketCategory? Category,
    Guid? AssignedAgentId,
    TicketStatus? Status
);