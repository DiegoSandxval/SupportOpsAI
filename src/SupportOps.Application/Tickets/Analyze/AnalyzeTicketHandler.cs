using SupportOps.Application.Abstractions.AI;
using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Domain.Enums;

namespace SupportOps.Application.Tickets.Analyze;

public sealed class AnalyzeTicketHandler
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketAiAnalyzer _ticketAiAnalyzer;

    public AnalyzeTicketHandler(
        ITicketRepository ticketRepository,
        ITicketAiAnalyzer ticketAiAnalyzer)
    {
        _ticketRepository = ticketRepository;
        _ticketAiAnalyzer = ticketAiAnalyzer;
    }

    public async Task<AnalyzeTicketResponse?> HandleAsync(
        Guid ticketId,
        Guid userId,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        var ticket =
            await _ticketRepository.GetByIdAsync(
                ticketId,
                cancellationToken
            );

        if (ticket is null)
        {
            return null;
        }

        if (role != UserRole.Agent &&
            role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException(
                "Only support staff can analyze tickets."
            );
        }

        var analysis =
            await _ticketAiAnalyzer.AnalyzeAsync(
                ticket.Title,
                ticket.Description,
                cancellationToken
            );

        return new AnalyzeTicketResponse(
            analysis.SuggestedCategory,
            analysis.SuggestedPriority,
            analysis.Summary,
            analysis.Reason
        );
    }
}