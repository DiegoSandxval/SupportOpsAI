namespace SupportOps.Application.Tickets.GetHistory;

public sealed record GetTicketHistoryResponse(
    Guid Id,
    Guid TicketId,
    Guid ChangedByUserId,
    string ChangedByUserName,
    string Action,
    string? PreviousValue,
    string? NewValue,
    string? Description,
    DateTime CreatedAtUtc
);