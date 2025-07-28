using BackendTracker.Ticket.NewFolder;
using BackendTrackerApplication.DTOs;
using BackendTrackerApplication.Exceptions;
using BackendTrackerDomain.Entity.Ticket;
using BackendTrackerDomain.Entity.Ticket.FileUpload;
using BackendTrackerDomain.Interfaces;
using Microsoft.Extensions.Logging;

namespace BackendTrackerApplication.Services;

public class TicketService(
    ITicketRepository _ticketRepository,
    IApplicationUserRepository _applicationUserRepository,
    ILogger<TicketService> logger)
{
    public async Task<IEnumerable<Ticket>> GetTickets(Guid submitterId)
    {
        if (submitterId == Guid.Empty)
        {
            logger.LogWarning("GetTickets called with an empty id.");
        }

        return await _ticketRepository.GetTickets(submitterId);
    }

    public async Task<TicketResponse> CreateTicket(TicketRequestBody request)
    {
        if (request.SubmitterId == Guid.Empty)
            throw new ArgumentException("Submitter ID is required");
        if (string.IsNullOrWhiteSpace(request.Title)) throw new ArgumentException("Title is required");

        var userExists = await _ticketRepository.UserExistsAsync(request.SubmitterId);
        if (!userExists)
            throw new UserNotFoundException(request.SubmitterId.ToString(), new Dictionary<string, string[]>());

        var user = await _applicationUserRepository.GetUserByIdAsync(request.SubmitterId) ??
                   throw new UserNotFoundException("User not found", new Dictionary<string, string[]>());

        var ticket = new Ticket
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
            Files = request.Files ?? new List<TicketFile>(),
            IsResolved = false,
            AssigneeId = null
        };

        var createdTicket = await _ticketRepository.CreateTicket(ticket);
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

    public async Task<TicketResponse> UpdateTicket(TicketRequestBody request)
    {
        if (request.SubmitterId == Guid.Empty)
            throw new ArgumentException("Submitter ID is required");

        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required");

        var userExists = await _ticketRepository.UserExistsAsync(request.SubmitterId);
        if (!userExists)
            throw new UserNotFoundException(request.SubmitterId, new Dictionary<string, string[]>());

        var ticket = new Ticket
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
            Files = request.Files ?? new List<TicketFile>(),
            IsResolved = false,
            Submitter = null,
            Assignee = null,
            AssigneeId = null
        };

        var createdTicket = await _ticketRepository.UpdateTicket(ticket);
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

        var deletedTicket = await _ticketRepository.DeleteAsync(ticketId);

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

    private static object? ConvertValue(object value, Type targetType)
    {
        if (value == null)
            return null;


        var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

        try
        {
            if (underlyingType.IsEnum)
            {
                if (value is string str)
                    return Enum.Parse(underlyingType, str, ignoreCase: true);
                else
                    return Enum.ToObject(underlyingType, value);
            }

            if (underlyingType == typeof(Guid))
            {
                if (value is string str)
                    return Guid.Parse(str);
                if (value is Guid g)
                    return g;
            }

            if (underlyingType == typeof(DateTime))
            {
                if (value is string str)
                    return DateTime.Parse(str);
                if (value is DateTime dt)
                    return dt;
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to convert value '{value}' to type '{targetType.Name}'", ex);
        }

        return Convert.ChangeType(value, underlyingType);
    }

    public async Task<TicketResponse> AssignTicketToUser(Guid userId, Guid ticketId)
    {
        if (userId == Guid.Empty || ticketId == Guid.Empty)
        {
            logger.LogWarning("AssignTicketToUser called with an empty user ID or ticket ID.");
        }

        var userExists = await _ticketRepository.UserExistsAsync(userId);
        if (!userExists)
        {
            logger.LogWarning($"User with ID {userId} does not exist.");
        }

        var ticket = await _ticketRepository.AssignTicketToUser(userId, ticketId);

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