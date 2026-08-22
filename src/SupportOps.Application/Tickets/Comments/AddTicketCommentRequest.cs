namespace SupportOps.Application.Tickets.Comments;

public sealed record AddTicketCommentRequest(
    string Message,
    bool IsInternal = false
);