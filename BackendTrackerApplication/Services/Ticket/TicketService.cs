using BackendTracker.Ticket.NewFolder;
using BackendTrackerApplication.DTOs;
using BackendTrackerApplication.Exceptions;
using BackendTrackerApplication.Services.Messaging;
using BackendTrackerDomain.Interfaces;
using Microsoft.Extensions.Logging;

namespace BackendTrackerApplication.Services.Ticket;

public class TicketService(
    ITicketRepository ticketRepository,
    IApplicationUserRepository applicationUserRepository,
    IMessageService messageService,
    ILogger<TicketService> logger)
{
    public async Task<IEnumerable<BackendTrackerDomain.Entity.Ticket.Ticket>> GetTickets(Guid submitterId)
    {
        if (submitterId == Guid.Empty)
        {
            logger.LogWarning("GetTickets called with an empty id.");
        }

        return await ticketRepository.GetTickets(submitterId);
    }

    public async Task<TicketResponse> CreateTicket(TicketRequestBody request)
    {
        if (request.SubmitterId == Guid.Empty)
            throw new ArgumentException("Submitter ID is required");
        if (string.IsNullOrWhiteSpace(request.Title)) throw new ArgumentException("Title is required");

        var userExists = await ticketRepository.UserExistsAsync(request.SubmitterId);
        if (!userExists)
            throw new UserNotFoundException(request.SubmitterId.ToString(), new Dictionary<string, string[]>());

        var user = await applicationUserRepository.GetUserByIdAsync(request.SubmitterId) ??
                   throw new UserNotFoundException("User not found", new Dictionary<string, string[]>());

        var ticket = new BackendTrackerDomain.Entity.Ticket.Ticket
        {
            TicketId = Guid.NewGuid(),
            Environment = request.Environment,
            Title = request.Title,
            Description = request.Description,
            StepsToReproduce = request.StepsToReproduce,
            ExpectedResult = request.ExpectedResult,
            SubmitterId = request.SubmitterId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Files = request.Files,
            IsResolved = false,
            AssigneeId = null 
        };

        var createdTicket = await ticketRepository.CreateTicket(ticket);
        await messageService.NotifyTicketCreated(createdTicket);
        return new TicketResponse
        {
            TicketId = createdTicket.TicketId,
            Title = createdTicket.Title,
            Description = createdTicket.Description,
            StepsToReproduce = createdTicket.StepsToReproduce,
            ExpectedResult = createdTicket.ExpectedResult,
            Environment = createdTicket.Environment.ToString(),
            SubmitterId = createdTicket.SubmitterId,
            CreatedAt = createdTicket.CreatedAt,
            IsResolved = createdTicket.IsResolved
        };
    }

    public async Task<TicketResponse> UpdateTicket(Guid ticketId, TicketRequestBody request)
    {
        if (request.SubmitterId == Guid.Empty)
            throw new ArgumentException("Submitter ID is required");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required");

        var userExists = await ticketRepository.UserExistsAsync(request.SubmitterId);
        if (!userExists)
            throw new UserNotFoundException(request.SubmitterId, new Dictionary<string, string[]>());
        
        var existingTicket = await ticketRepository.GetTicketById(ticketId);
        if (existingTicket == null)
            throw new TicketExceptions("Ticket not found with id: " + ticketId);
        

        var ticket = new BackendTrackerDomain.Entity.Ticket.Ticket
        {
            TicketId = existingTicket.TicketId,
            Environment = request.Environment,
            Title = request.Title,
            Description = request.Description,
            StepsToReproduce = request.StepsToReproduce,
            ExpectedResult = request.ExpectedResult,
            SubmitterId = request.SubmitterId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Files = request.Files,
            IsResolved = false,
            Submitter = null,
            Assignee = null,
            AssigneeId = null
        };

        var createdTicket = await ticketRepository.UpdateTicket(ticket);
        await messageService.NotifyTicketUpdated(createdTicket);
        return new TicketResponse
        {
            TicketId = createdTicket.TicketId,
            Title = createdTicket.Title,
            Description = createdTicket.Description,
            StepsToReproduce = createdTicket.StepsToReproduce,
            ExpectedResult = createdTicket.ExpectedResult,
            Environment = createdTicket.Environment.ToString(),
            SubmitterId = createdTicket.SubmitterId,
            CreatedAt = createdTicket.CreatedAt,
            IsResolved = createdTicket.IsResolved
        };
    }

    public async Task<TicketResponse> DeleteTicket(Guid ticketId)
    {
        if (ticketId.Equals(Guid.Empty))
        {
            logger.LogWarning("DeleteTicket called with an empty ticket ID.");
            throw new ArgumentException("Ticket ID is required for deletion.");
        }

        var deletedTicket = await ticketRepository.DeleteAsync(ticketId);

        return new TicketResponse
        {
            TicketId = deletedTicket.TicketId,
            Title = deletedTicket.Title,
            Description = deletedTicket.Description,
            StepsToReproduce = deletedTicket.StepsToReproduce,
            ExpectedResult = deletedTicket.ExpectedResult,
            Environment = deletedTicket.Environment.ToString(),
            SubmitterId = deletedTicket.SubmitterId,
            CreatedAt = deletedTicket.CreatedAt
        };
    }

    public async Task<TicketResponse> AssignTicketToUser(Guid userId, Guid ticketId)
    {
        if (userId == Guid.Empty || ticketId == Guid.Empty)
        {
            logger.LogWarning("AssignTicketToUser called with an empty user ID or ticket ID.");
        }

        var userExists = await ticketRepository.UserExistsAsync(userId);
        if (!userExists)
        {
            logger.LogWarning($"User with ID {userId} does not exist.");
        }

        var ticket = await ticketRepository.AssignTicketToUser(userId, ticketId);

        return new TicketResponse
        {
            TicketId = ticket.TicketId,
            Title = ticket.Title,
            Description = ticket.Description,
            StepsToReproduce = ticket.StepsToReproduce,
            ExpectedResult = ticket.ExpectedResult,
            Environment = ticket.Environment.ToString(),
            SubmitterId = ticket.SubmitterId,
            CreatedAt = ticket.CreatedAt
        };
    }
}