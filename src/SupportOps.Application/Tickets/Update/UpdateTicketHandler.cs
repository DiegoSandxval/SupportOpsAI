using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Domain.Entities;
using SupportOps.Domain.Enums;

namespace SupportOps.Application.Tickets.Update;

public sealed class UpdateTicketHandler
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITicketHistoryRepository _historyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTicketHandler(
        ITicketRepository ticketRepository,
        IUserRepository userRepository,
        ITicketHistoryRepository historyRepository,
        IUnitOfWork unitOfWork)
    {
        _ticketRepository = ticketRepository;
        _userRepository = userRepository;
        _historyRepository = historyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HandleAsync(
        UpdateTicketCommand command,
        Guid userId,
        UserRole role,
        CancellationToken cancellationToken = default)
    {
        var ticket = await _ticketRepository.GetByIdAsync(
            command.TicketId,
            cancellationToken
        );

        if (ticket is null)
        {
            return false;
        }

        // User solamente puede modificar sus propios tickets.
        if (role == UserRole.User &&
            ticket.CreatedByUserId != userId)
        {
            return false;
        }

        if (role != UserRole.User &&
            role != UserRole.Agent &&
            role != UserRole.Admin)
        {
            return false;
        }

        if (command.Priority.HasValue &&
            command.Priority.Value != ticket.Priority)
        {
            ticket.ChangePriority(
                command.Priority.Value
            );

            await AddHistoryAsync(
                ticket.Id,
                userId,
                TicketHistoryAction.PriorityChanged,
                command.Priority.Value.ToString(),
                "Ticket priority changed.",
                cancellationToken
            );
        }

        if (command.Category.HasValue &&
            command.Category.Value != ticket.Category)
        {
            ticket.ChangeCategory(
                command.Category.Value
            );

            await AddHistoryAsync(
                ticket.Id,
                userId,
                TicketHistoryAction.CategoryChanged,
                command.Category.Value.ToString(),
                "Ticket category changed.",
                cancellationToken
            );
        }

        // Un User normal no puede asignar agentes.
        if (command.AssignedAgentId.HasValue &&
       command.AssignedAgentId.Value != ticket.AssignedAgentId)
        {
            if (role == UserRole.User)
            {
                throw new UnauthorizedAccessException(
                    "Users cannot assign agents to tickets."
                );
            }

            var agent = await _userRepository.GetByIdAsync(
                command.AssignedAgentId.Value,
                cancellationToken
            );

            if (agent is null)
            {
                throw new InvalidOperationException(
                    "The selected agent does not exist."
                );
            }

            if (!agent.IsActive)
            {
                throw new InvalidOperationException(
                    "The selected agent is inactive."
                );
            }

            if (agent.Role != UserRole.Agent)
            {
                throw new InvalidOperationException(
                    "The selected user is not an agent."
                );
            }

            ticket.AssignTo(agent.Id);

            await AddHistoryAsync(
                ticket.Id,
                userId,
                TicketHistoryAction.Assigned,
                agent.Id.ToString(),
                $"Ticket assigned to {agent.GetFullName()}.",
                cancellationToken
            );
        }
        if (command.Status.HasValue &&
            command.Status.Value != ticket.Status)
        {
            // Un User normal no puede cambiar estados.
            if (role == UserRole.User)
            {
                throw new UnauthorizedAccessException(
                    "Users cannot change ticket status."
                );
            }

            switch (command.Status.Value)
            {
                case TicketStatus.InProgress:
                    ticket.StartProgress();

                    await AddHistoryAsync(
                        ticket.Id,
                        userId,
                        TicketHistoryAction.StatusChanged,
                        TicketStatus.InProgress.ToString(),
                        "Ticket moved to in progress.",
                        cancellationToken
                    );

                    break;

                case TicketStatus.Resolved:
                    ticket.Resolve();

                    await AddHistoryAsync(
                        ticket.Id,
                        userId,
                        TicketHistoryAction.Resolved,
                        TicketStatus.Resolved.ToString(),
                        "Ticket resolved.",
                        cancellationToken
                    );

                    break;

                case TicketStatus.Closed:
                    ticket.Close();

                    await AddHistoryAsync(
                        ticket.Id,
                        userId,
                        TicketHistoryAction.Closed,
                        TicketStatus.Closed.ToString(),
                        "Ticket closed.",
                        cancellationToken
                    );

                    break;

                default:
                    throw new InvalidOperationException(
                        $"Status '{command.Status.Value}' cannot be set directly."
                    );
            }
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken
        );

        return true;
    }

    private async Task AddHistoryAsync(
        Guid ticketId,
        Guid userId,
        TicketHistoryAction action,
        string newValue,
        string description,
        CancellationToken cancellationToken)
    {
        var history = new TicketHistory(
            ticketId,
            userId,
            action,
            newValue: newValue,
            description: description
        );

        await _historyRepository.AddAsync(
            history,
            cancellationToken
        );
    }
}