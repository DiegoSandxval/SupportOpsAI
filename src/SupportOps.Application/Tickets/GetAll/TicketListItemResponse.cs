using System;
using System.Collections.Generic;
using System.Text;

namespace SupportOps.Application.Tickets.GetAll;

public sealed record TicketListItemResponse(
    Guid Id,
    string Title,
    string Status,
    string Priority,
    string Category,
    Guid CreatedByUserId,
    Guid? AssignedAgentId,
    DateTime CreatedAtUtc
);