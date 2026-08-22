using SupportOps.Domain.Enums;

namespace SupportOps.Application.Tickets.Analyze;

public sealed record AnalyzeTicketResponse(
    TicketCategory SuggestedCategory,
    TicketPriority SuggestedPriority,
    string Summary,
    string Reason
);