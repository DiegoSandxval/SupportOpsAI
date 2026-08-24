using SupportOps.Application.Abstractions.Persistence;

namespace SupportOps.Application.Users.GetAgents;

public sealed class GetAgentsHandler
{
    private readonly IUserRepository _userRepository;

    public GetAgentsHandler(
        IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<IReadOnlyList<AgentListItemResponse>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        var agents =
            await _userRepository.GetActiveAgentsAsync(
                cancellationToken
            );

        return agents
            .Select(agent =>
                new AgentListItemResponse(
                    agent.Id,
                    agent.GetFullName(),
                    agent.Email
                ))
            .ToList();
    }
}