using SupportOps.Domain.Enums;

namespace SupportOps.Application.Abstractions.AI;

public interface ITicketAiAnalyzer
{
    Task<TicketAiAnalysisResult> AnalyzeAsync(
        string title,
        string description,
        CancellationToken cancellationToken = default);
}

public sealed record TicketAiAnalysisResult(
    TicketCategory SuggestedCategory,
    TicketPriority SuggestedPriority,
    string Summary,
    string Reason
);