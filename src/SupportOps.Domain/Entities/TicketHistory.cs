using SupportOps.Domain.Common;
using SupportOps.Domain.Enums;

namespace SupportOps.Domain.Entities;

public class TicketHistory : Entity
{
    public Guid TicketId { get; private set; }

    public Guid ChangedByUserId { get; private set; }

    public TicketHistoryAction Action { get; private set; }

    public string? PreviousValue { get; private set; }

    public string? NewValue { get; private set; }

    public string? Description { get; private set; }

    private TicketHistory()
    {
    }

    public TicketHistory(
        Guid ticketId,
        Guid changedByUserId,
        TicketHistoryAction action,
        string? previousValue = null,
        string? newValue = null,
        string? description = null)
    {
        if (ticketId == Guid.Empty)
        {
            throw new ArgumentException(
                "Ticket ID is required.",
                nameof(ticketId)
            );
        }

        if (changedByUserId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID is required.",
                nameof(changedByUserId)
            );
        }

        TicketId = ticketId;
        ChangedByUserId = changedByUserId;
        Action = action;

        PreviousValue = previousValue?.Trim();
        NewValue = newValue?.Trim();
        Description = description?.Trim();
    }
}