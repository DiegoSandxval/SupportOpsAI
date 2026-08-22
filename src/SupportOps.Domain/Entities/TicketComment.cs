using System;
using System.Collections.Generic;
using SupportOps.Domain.Common;

namespace SupportOps.Domain.Entities;

public class TicketComment : Entity
{
    public Guid TicketId { get; private set; }

    public Guid UserId { get; private set; }

    public string Message { get; private set; }

    public bool IsInternal { get; private set; }

    private TicketComment()
    {
        Message = string.Empty;
    }

    public TicketComment(
        Guid ticketId,
        Guid userId,
        string message,
        bool isInternal = false)
    {
        if (ticketId == Guid.Empty)
        {
            throw new ArgumentException(
                "Ticket ID is required.",
                nameof(ticketId)
            );
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException(
                "User ID is required.",
                nameof(userId)
            );
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "Comment message is required.",
                nameof(message)
            );
        }

        TicketId = ticketId;
        UserId = userId;
        Message = message.Trim();
        IsInternal = isInternal;
    }

    public void UpdateMessage(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "Comment message is required.",
                nameof(message)
            );
        }

        Message = message.Trim();

        MarkAsUpdated();
    }
}