using SupportOps.Domain.Common;
using SupportOps.Domain.Enums;

namespace SupportOps.Domain.Entities;

public class Ticket : Entity
{
    public string Title { get; private set; }

    public string Description { get; private set; }

    public TicketStatus Status { get; private set; }

    public TicketPriority Priority { get; private set; }

    public TicketCategory Category { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public Guid? AssignedAgentId { get; private set; }

    public DateTime? ResolvedAtUtc { get; private set; }

    public DateTime? ClosedAtUtc { get; private set; }

    private Ticket()
    {
        Title = string.Empty;
        Description = string.Empty;
    }

    public Ticket(
        string title,
        string description,
        Guid createdByUserId,
        TicketPriority priority = TicketPriority.Medium,
        TicketCategory category = TicketCategory.General)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException(
                "Ticket title is required.",
                nameof(title)
            );
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException(
                "Ticket description is required.",
                nameof(description)
            );
        }

        Title = title.Trim();
        Description = description.Trim();
        CreatedByUserId = createdByUserId;

        Priority = priority;
        Category = category;

        Status = TicketStatus.Open;
    }

    public void AssignTo(Guid agentId)
    {
        if (agentId == Guid.Empty)
        {
            throw new ArgumentException(
                "Agent ID is required.",
                nameof(agentId)
            );
        }

        if (Status == TicketStatus.Closed)
        {
            throw new InvalidOperationException(
                "A closed ticket cannot be assigned."
            );
        }

        AssignedAgentId = agentId;

        if (Status == TicketStatus.Open)
        {
            Status = TicketStatus.Assigned;
        }

        MarkAsUpdated();
    }

    public void StartProgress()
    {
        if (AssignedAgentId is null)
        {
            throw new InvalidOperationException(
                "The ticket must have an assigned agent before work can begin."
            );
        }

        if (Status == TicketStatus.Closed)
        {
            throw new InvalidOperationException(
                "A closed ticket cannot be started."
            );
        }

        Status = TicketStatus.InProgress;

        MarkAsUpdated();
    }

    public void ChangePriority(TicketPriority priority)
    {
        Priority = priority;

        MarkAsUpdated();
    }

    public void ChangeCategory(TicketCategory category)
    {
        Category = category;

        MarkAsUpdated();
    }

    public void Resolve()
    {
        if (Status == TicketStatus.Closed)
        {
            throw new InvalidOperationException(
                "A closed ticket cannot be resolved."
            );
        }

        Status = TicketStatus.Resolved;
        ResolvedAtUtc = DateTime.UtcNow;

        MarkAsUpdated();
    }

    public void Close()
    {
        if (Status != TicketStatus.Resolved)
        {
            throw new InvalidOperationException(
                "Only resolved tickets can be closed."
            );
        }

        Status = TicketStatus.Closed;
        ClosedAtUtc = DateTime.UtcNow;

        MarkAsUpdated();
    }
}