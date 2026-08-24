using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Domain.Entities;
using SupportOps.Domain.Enums;

namespace SupportOps.Application.Tickets.Comments;

public sealed class AddTicketCommentHandler
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketCommentRepository _commentRepository;
    private readonly ITicketHistoryRepository _historyRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserRepository _userRepository;

    public AddTicketCommentHandler(
        ITicketRepository ticketRepository,
        ITicketCommentRepository commentRepository,
        ITicketHistoryRepository historyRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _commentRepository = commentRepository;
        _historyRepository = historyRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TicketCommentResponse?> HandleAsync(
        Guid ticketId,
        Guid userId,
        UserRole role,
        AddTicketCommentRequest request,
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

        // Normal users can only access their own tickets.
        if (role == UserRole.User &&
            ticket.CreatedByUserId != userId)
        {
            return null;
        }

        // Normal users cannot create internal comments.
        if (role == UserRole.User &&
            request.IsInternal)
        {
            throw new UnauthorizedAccessException(
                "Users cannot create internal comments."
            );
        }

        if (role != UserRole.User &&
            role != UserRole.Agent &&
            role != UserRole.Admin)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to comment on this ticket."
            );
        }

        var comment = new TicketComment(
            ticketId,
            userId,
            request.Message,
            request.IsInternal
        );

        await _commentRepository.AddAsync(
            comment,
            cancellationToken
        );

        var history = new TicketHistory(
            ticketId,
            userId,
            TicketHistoryAction.CommentAdded,
            description: request.IsInternal
                ? "Internal comment added."
                : "Comment added."
        );

        await _historyRepository.AddAsync(
            history,
            cancellationToken
        );

        await _unitOfWork.SaveChangesAsync(
            cancellationToken
        );
        var commentUser =
            await _userRepository.GetByIdAsync(
                userId,
                cancellationToken
            );

        var userFullName =
            commentUser?.GetFullName();

        return new TicketCommentResponse(
            comment.Id,
            comment.TicketId,
            comment.UserId,
            userFullName,
            comment.Message,
            comment.IsInternal,
            comment.CreatedAtUtc
        );
    }
}