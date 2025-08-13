using BackendTrackerApplication.Dtos;
using BackendTrackerDomain.Entity.Message;
using BackendTrackerDomain.Entity.Ticket;
using Microsoft.AspNetCore.SignalR;

namespace BackendTrackerApplication.Services.Messaging;

public class MessageService(IHubContext<MessageHub> hubContext) : IMessageService
{
    public async Task NotifyTicketCreated(BackendTrackerDomain.Entity.Ticket.Ticket ticket)
    {
        if (ticket.AssigneeId.HasValue)
        {
            await hubContext.Clients.User(ticket.AssigneeId.ToString()).SendAsync("TicketCreated", new
            {
                Id = ticket.TicketId,
                Title = ticket.Title,
                Description = ticket.Description,
                environment = ticket.Environment
            });
        }
        else
        {
            await hubContext.Clients.Group("Admins").SendAsync("UnassignedTicketCreated", new
            {
                Id = ticket.TicketId,
                Title = ticket.Title,
                Description = ticket.Description,
                environment = ticket.Environment
            });
        }
    }

    public async Task NotifyTicketUpdated(BackendTrackerDomain.Entity.Ticket.Ticket ticket)
    {
        if (ticket.AssigneeId.HasValue)
        {
            await hubContext.Clients.User(ticket.SubmitterId.ToString()).SendAsync("TicketUpdated", new
            {
                Id = ticket.TicketId,
                Title = ticket.Title,
                Description = ticket.Description,
                environment = ticket.Environment
            });
            
            await hubContext.Clients.Group("Admins").SendAsync("TicketUpdated", new
            {
                Id = ticket.TicketId,
                Title = ticket.Title,
                Description = ticket.Description,
                environment = ticket.Environment
            });
        }
        else
        {
            await hubContext.Clients.Group("Admins").SendAsync("TicketUpdated", new
            {
                Id = ticket.TicketId,
                Title = ticket.Title,
                Description = ticket.Description,
                environment = ticket.Environment
            });
        }
    }

    public Task NotifyTicketAssigned(BackendTrackerDomain.Entity.Ticket.Ticket ticket, Guid assigneeId)
    {
        return hubContext.Clients.User(assigneeId.ToString()).SendAsync("TicketAssigned", new
        {
            Id = ticket.TicketId,
            Title = ticket.Title,
            Description = ticket.Description,
            environment = ticket.Environment
        });
    }

    public Task SendAsync(MessageDto message)
    {
        return hubContext.Clients.All.SendAsync("MessageReceived", new
        {
            Content = message.Content,
            SentTime = message.SentTime,
            isRead = message.IsRead,
        });
    }
}