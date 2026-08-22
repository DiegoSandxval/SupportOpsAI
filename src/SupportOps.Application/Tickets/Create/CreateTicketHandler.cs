using SupportOps.Application.Abstractions.Persistence;
using SupportOps.Domain.Entities;
using SupportOps.Domain.Enums;

namespace SupportOps.Application.Tickets.Create;

public sealed class CreateTicketHandler
{
    private readonly IUserRepository _userRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketHistoryRepository _historyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTicketHandler(
        IUserRepository userRepository,
        ITicketRepository ticketRepository,
        ITicketHistoryRepository historyRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _ticketRepository = ticketRepository;
        _historyRepository = historyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CreateTicketResponse> HandleAsync(
        Guid userId,
        CreateTicketRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(
            userId,
            cancellationToken
        );

        if (user is null)
        {
            throw new InvalidOperationException(
                "Authenticated user was not found."
            );
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException(
                "Inactive users cannot create tickets."
            );
        }

        var ticket = new Ticket(
            request.Title,
            request.Description,
            user.Id,
            request.Priority,
            request.Category
        );

        await _ticketRepository.AddAsync(
            ticket,
            cancellationToken
        );

        var history = new TicketHistory(
            ticket.Id,
            user.Id,
            TicketHistoryAction.Created,
            newValue: TicketStatus.Open.ToString(),
            description: "Ticket created."
        );

        await _historyRepository.AddAsync(
            history,
            cancellationToken
        );

        await _unitOfWork.SaveChangesAsync(
            cancellationToken
        );

        return new CreateTicketResponse(
            ticket.Id,
            ticket.Title,
            ticket.Description,
            ticket.Status.ToString(),
            ticket.Priority.ToString(),
            ticket.Category.ToString(),
            ticket.CreatedByUserId,
            ticket.CreatedAtUtc
        );
    }
}