using System;
using System.Collections.Generic;
using SupportOps.Domain.Enums;

namespace SupportOps.Application.Tickets.Create;

public sealed record CreateTicketRequest(
    string Title,
    string Description,
    TicketPriority Priority,
    TicketCategory Category
);