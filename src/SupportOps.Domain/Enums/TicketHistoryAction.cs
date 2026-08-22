using System;
using System.Collections.Generic;
using System.Text;

namespace SupportOps.Domain.Enums;

public enum TicketHistoryAction
{
    Created = 1,
    Assigned = 2,
    StatusChanged = 3,
    PriorityChanged = 4,
    CategoryChanged = 5,
    CommentAdded = 6,
    Resolved = 7,
    Closed = 8
}