namespace SupportOps.Application.Users.GetAgents;

public sealed record AgentListItemResponse(
    Guid Id,
    string FullName,
    string Email
);