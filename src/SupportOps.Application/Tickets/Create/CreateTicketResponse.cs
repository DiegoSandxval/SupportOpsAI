using System;
using System.Collections.Generic;
using System.Text;

namespace SupportOps.Application.Tickets.Create;

public sealed record CreateTicketResponse(
    Guid Id,
    string Title,
    string Description,
    string Status,
    string Priority,
    string Category,
    Guid CreatedByUserId,
    DateTime CreatedAtUtc
);