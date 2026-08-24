namespace SupportOps.Application.Tickets.Comments;

public sealed record TicketCommentResponse(
    Guid Id,
    Guid TicketId,
    Guid UserId,
    string UserFullName,
    string Message,
    bool IsInternal,
    DateTime CreatedAtUtc
);