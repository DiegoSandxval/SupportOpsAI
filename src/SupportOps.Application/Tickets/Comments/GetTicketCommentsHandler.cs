using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Domain.Enums;

namespace SupportOps.Application.Tickets.Comments;

public sealed class GetTicketCommentsHandler
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketCommentRepository _commentRepository;

    public GetTicketCommentsHandler(
        ITicketRepository ticketRepository,
        ITicketCommentRepository commentRepository)
    {
        _ticketRepository = ticketRepository;
        _commentRepository = commentRepository;
    }

    public async Task<IReadOnlyList<TicketCommentResponse>?> HandleAsync(
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

        if (role == UserRole.User &&
            ticket.CreatedByUserId != userId)
        {
            return null;
        }

        if (role != UserRole.User &&
            role != UserRole.Agent &&
            role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException();
        }

        var comments =
            await _commentRepository.GetByTicketIdAsync(
                ticketId,
                cancellationToken
            );

        // Users must never see internal staff comments.
        var visibleComments =
            role == UserRole.User
                ? comments.Where(x => !x.IsInternal)
                : comments;

        return visibleComments
            .Select(comment =>
                new TicketCommentResponse(
                    comment.Id,
                    comment.TicketId,
                    comment.UserId,
                    comment.Message,
                    comment.IsInternal,
                    comment.CreatedAtUtc
                ))
            .ToList();
    }
}