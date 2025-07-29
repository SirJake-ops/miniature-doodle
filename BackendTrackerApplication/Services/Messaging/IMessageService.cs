using BackendTrackerDomain.Entity.Message;
using BackendTrackerDomain.Entity.Ticket;

namespace BackendTrackerApplication.Services.Messaging;

public interface IMessageService
{
    Task NotifyTicketCreated(Ticket ticket);
    Task NotifyTicketUpdated(Ticket ticket);
    Task NotifyTicketAssigned(Ticket ticket, Guid assigneeId);
    Task SendAsync(Message message);
}