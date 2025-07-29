using BackendTrackerDomain.Entity.Message;
using BackendTrackerDomain.Entity.Ticket;
using Microsoft.AspNetCore.SignalR;

namespace BackendTrackerApplication.Services.Messaging;

public class MessageService(IHubContext<MessageHub> hubContext) : IMessageService
{
    public async Task NotifyTicketCreated(Ticket ticket)
    {
        await hubContext.Clients.User(ticket.AssigneeId.ToString()).SendAsync("TicketCreated", new
        {
            Id = ticket.TicketId,
            Title = ticket.Title,
            Description = ticket.Description,
            environment = ticket.Environment
        });
    }

    public async Task NotifyTicketUpdated(Ticket ticket)
    {
        await hubContext.Clients.All.SendAsync("TicketUpdated", new
        {
            Id = ticket.TicketId,
            Title = ticket.Title,
            Description = ticket.Description,
            environment = ticket.Environment
        });
    }

    public Task NotifyTicketAssigned(Ticket ticket, Guid assigneeId)
    {
        return hubContext.Clients.User(assigneeId.ToString()).SendAsync("TicketAssigned", new
        {
            Id = ticket.TicketId,
            Title = ticket.Title,
            Description = ticket.Description,
            environment = ticket.Environment
        });
    }

    public Task SendAsync(Message message)
    {
        return hubContext.Clients.All.SendAsync("MessageReceived", new
        {
            Id = message.Id,
            Content = message.Content,
            SenderId = message.SenderId,
            Timestamp = message.SentTime
        });
    }
}