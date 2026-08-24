using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Domain.Enums;

namespace SupportOps.Application.Tickets.Comments;

public sealed class GetTicketCommentsHandler
{
    private readonly ITicketRepository
        _ticketRepository;

    private readonly ITicketCommentRepository
        _commentRepository;

    private readonly IUserRepository
        _userRepository;

    public GetTicketCommentsHandler(
        ITicketRepository ticketRepository,
        ITicketCommentRepository commentRepository,
        IUserRepository userRepository)
    {
        _ticketRepository =
            ticketRepository;

        _commentRepository =
            commentRepository;

        _userRepository =
            userRepository;
    }

    public async Task<
        IReadOnlyList<TicketCommentResponse>?
    > HandleAsync(
        Guid ticketId,
        Guid userId,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        var ticket =
            await _ticketRepository
                .GetByIdAsync(
                    ticketId,
                    cancellationToken
                );

        if (ticket is null)
        {
            return null;
        }

        if (
            role == UserRole.User &&
            ticket.CreatedByUserId != userId
        )
        {
            return null;
        }

        if (
            role != UserRole.User &&
            role != UserRole.Agent &&
            role != UserRole.Admin
        )
        {
            throw new UnauthorizedAccessException();
        }

        var comments =
            await _commentRepository
                .GetByTicketIdAsync(
                    ticketId,
                    cancellationToken
                );

        var visibleComments =
            (
                role == UserRole.User
                    ? comments.Where(
                        comment =>
                            !comment.IsInternal
                    )
                    : comments
            )
            .ToList();

        var userIds =
            visibleComments
                .Select(
                    comment =>
                        comment.UserId
                )
                .Distinct()
                .ToArray();

        var users =
            await _userRepository
                .GetByIdsAsync(
                    userIds,
                    cancellationToken
                );

        var userNames =
            users.ToDictionary(
                user => user.Id,
                user =>
                    user.GetFullName()
            );

        return visibleComments
            .Select(comment =>
            {
                var userFullName =
                    userNames.TryGetValue(
                        comment.UserId,
                        out var name
                    )
                        ? name
                        : "Unknown User";

                return new TicketCommentResponse(
                    comment.Id,
                    comment.TicketId,
                    comment.UserId,
                    userFullName,
                    comment.Message,
                    comment.IsInternal,
                    comment.CreatedAtUtc
                );
            })
            .ToList();
    }
}