namespace SupportOps.Application.Tickets.GetTicketById;

public sealed record GetTicketByIdResponse(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    string Category,
    Guid CreatedByUserId,
    Guid? AssignedAgentId,
    DateTime CreatedAtUtc,
    DateTime? ResolvedAtUtc,
    DateTime? ClosedAtUtc
);